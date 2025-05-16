using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace todo_service;

public class EventProducerService : IDisposable
{
    private readonly ProducerConfig producerConfig;

    private readonly IProducer<Null, string> producer;

    public EventProducerService(IOptions<Configuration> configuration)
    {
        producerConfig = new()
        {
            BootstrapServers = configuration.Value.KafkaBootstrapServer
        };

        producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public void Dispose()
    {
        producer.Dispose();
    }

    public async Task SendMessageAsync(string message)
    {
        await producer.ProduceAsync("todo", new Message<Null, string> { Value = message });
    }
}
