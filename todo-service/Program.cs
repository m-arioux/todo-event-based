using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using todo_service;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddOptions<Configuration>()
    .Bind(builder.Configuration)
    .ValidateOnStart()
    .ValidateDataAnnotations();

builder.Services.AddSingleton<EventProducerService>();
builder.Services.AddHostedService<EventConsumerService>();
builder.Services.AddSingleton<EventListeningService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// await app.Services.GetRequiredService<EventProducerService>().SendMessageAsync("test message");

app.MapGet("/", () => "Hello World from todo-service");

app.MapPost("/todo", async ([FromServices] EventProducerService producer, [FromBody] Todo todo) =>
{
    await producer.SendMessageAsync(todo.Description);
});

app.MapGet("/todo-live", async (HttpContext context, [FromServices] EventListeningService eventListeningService, ILogger<Program> logger, CancellationToken ct) =>
{
    context.Response.Headers.Add("Content-Type", "text/event-stream");
    context.Response.Headers.Add("Content-Encoding", "none");

    var listenerErrorCt = CancellationTokenSource.CreateLinkedTokenSource(ct);

    var listener = new Listener(async (value) =>
    {
        logger.LogInformation("Sending back as SSE {event}", value);

        try
        {
            await context.Response.WriteAsync($"data: ");
            await JsonSerializer.SerializeAsync(context.Response.Body, new Todo { Description = value });
            await context.Response.WriteAsync($"\n\n");
            await context.Response.Body.FlushAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception while trying to send SSE");
            listenerErrorCt.Cancel();
        }

    });

    var unsubscribe = eventListeningService.RegisterListener(listener);

    while (!ct.IsCancellationRequested && !context.RequestAborted.IsCancellationRequested && !listenerErrorCt.IsCancellationRequested)
    {
        await Task.Delay(1000, ct);
    }

    unsubscribe();
});

app.Run();


record Todo
{
    [Required(AllowEmptyStrings = false)]
    public required string Description { get; set; }
}