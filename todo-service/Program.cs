using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
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

app.MapGet("/todo-live", async (HttpContext context, [FromServices] TodoListeningService eventListeningService, IOptions<JsonOptions> jsonOptions, ILogger<Program> logger) =>
{
    context.Response.Headers.Add("Content-Type", "text/event-stream");
    context.Response.Headers.Add("Content-Encoding", "none");
    context.Response.Headers.Add("Connection", "keep-alive");

    var clientId = context.Request.Headers.Origin.FirstOrDefault() + " / " + context.Request.Headers.UserAgent.FirstOrDefault();

    await context.Response.WriteAsync($":\n\n");
    await context.Response.Body.FlushAsync();

    await context.Response.Body.FlushAsync();

    var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
    var cancellationToken = cancellationTokenSource.Token;

    var listener = new Listener(async (todo) =>
    {
        logger.LogTrace("Sending back as SSE {event} to {id}", todo, clientId);

        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogTrace("Tried to send SSE event but the request was cancelled - {id}", clientId);

                return;
            }

            var json = JsonSerializer.Serialize(todo, jsonOptions.Value.JsonSerializerOptions);

            await context.Response.WriteAsync($"data: {json}\n\n");
            await context.Response.Body.FlushAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "error while ending SSE to {id}", clientId);
            cancellationTokenSource.Cancel();
        }

    });

    var unsubscribe = eventListeningService.RegisterListener(listener);

    while (!cancellationToken.IsCancellationRequested)
    {
        logger.LogTrace("Sending keep-alive to {id}", clientId);
        // sending SSE comment to keep client alive
        await context.Response.WriteAsync($":\n\n");
        await context.Response.Body.FlushAsync();

        await Task.Delay(10000);
    }

    logger.LogTrace("closed connection to {id} - {cancelled}", clientId, cancellationToken.IsCancellationRequested);

    unsubscribe();
});

app.Run();


public record Todo
{
    [Required(AllowEmptyStrings = false)]
    public required string Description { get; set; }

    public Guid? Id { get; set; }
}

public class EventStream<T>
{
    private readonly Channel<T> channel = Channel.CreateUnbounded<T>();

    public void OnEventReceived(T value)
    {
        channel.Writer.TryWrite(value);
    }
}