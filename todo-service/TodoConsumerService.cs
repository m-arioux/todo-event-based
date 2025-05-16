using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace todo_service;

/// <summary>
/// This class has the purpose to be a middle-man between the HTTP Responses in Server-Side Events and the Kafka Events.
/// With this, many consumers (e.g many front-ends) can subscribe to the same Kafka Consumer
/// </summary>
/// <param name="eventListeningService"></param>
/// <param name="configuration"></param>
/// <param name="logger"></param>
public class TodoConsumerService(
    TodoListeningService eventListeningService,
    IOptions<Configuration> configuration,
    ILogger<TodoConsumerService> logger) : BackgroundService
{
    private IConsumer<string, string> consumer;


    private async Task StartConsumerLoop(CancellationToken ct)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration.Value.KafkaBootstrapServer,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            GroupId = "group",
        };

        consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("todo");


        while (!ct.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(ct);

                var todo = new Todo { Id = Guid.Parse(cr.Message.Key), Description = cr.Message.Value };

                await eventListeningService.Dispatch(todo);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                logger.LogError("Consume error: {error}", e.Error.Reason);

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unexpected error");
                break;
            }
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }

    public override void Dispose()
    {
        consumer?.Close(); // Commit offsets and leave the group cleanly.
        consumer?.Dispose();

        base.Dispose();

        GC.SuppressFinalize(this);
    }


}

