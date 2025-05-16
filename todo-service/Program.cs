using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

builder.Services.AddSingleton<TodoProducerService>();
builder.Services.AddHostedService<TodoConsumerService>();
builder.Services.AddSingleton<TodoListeningService>();

// builder.Services.ConfigureHttpJsonOptions(x => x.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// await app.Services.GetRequiredService<EventProducerService>().SendMessageAsync("test message");

app.MapGet("/", () => "Hello World from todo-service");

app.MapPost("/todo", async ([FromServices] TodoProducerService producer, [FromBody] Todo todo) =>
{
    if (todo is null)
    {
        return;
    }

    todo.Id = Guid.NewGuid();

    await producer.SendMessageAsync(todo);
});

app.MapGet("/todo-live", async (HttpContext context, [FromServices] TodoListeningService eventListeningService, IOptions<JsonOptions> jsonOptions, ILogger<Program> logger, CancellationToken ct) =>
{
    context.Response.Headers.Add("Content-Type", "text/event-stream");
    context.Response.Headers.Add("Content-Encoding", "none");

    var listenerErrorCt = CancellationTokenSource.CreateLinkedTokenSource(ct);

    var listener = new Listener(async (todo) =>
    {
        logger.LogInformation("Sending back as SSE {event}", todo);

        try
        {
            await context.Response.WriteAsync($"data: ");
            await JsonSerializer.SerializeAsync(context.Response.Body, todo, jsonOptions.Value.JsonSerializerOptions);
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


public record Todo
{
    [Required(AllowEmptyStrings = false)]
    public required string Description { get; set; }

    public Guid? Id { get; set; }
}