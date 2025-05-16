using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace todo_service;

public class EventConsumerService(
    EventListeningService eventListeningService,
    IOptions<Configuration> configuration,
    ILogger<EventConsumerService> logger) : BackgroundService
{
    private IConsumer<Ignore, string> consumer;


    private async Task StartConsumerLoop(CancellationToken ct)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration.Value.KafkaBootstrapServer,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            GroupId = "group"
        };

        consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("todo");


        while (!ct.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(ct);

                await eventListeningService.Dispatch(cr.Message.Value);
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

