using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;
using Microsoft.Extensions.Options;

namespace todo_service;

public class TodoProducerService : IDisposable
{
    private readonly ProducerConfig producerConfig;

    private readonly IProducer<string, string> producer;

    public TodoProducerService(IOptions<Configuration> configuration)
    {
        producerConfig = new()
        {
            BootstrapServers = configuration.Value.KafkaBootstrapServer
        };

        producer = new ProducerBuilder<string, string>(producerConfig).BuildWithInstrumentation();
    }

    public void Dispose()
    {
        producer.Dispose();
    }

    public async Task SendMessageAsync(Todo todo)
    {
        await producer.ProduceAsync("todo", new Message<string, string> { Key = todo.Id.Value.ToString(), Value = todo.Description });
    }
}
