namespace todo_service;

public class EventListeningService(ILogger<EventListeningService> logger)
{
    private readonly List<Listener> listeners = [];

    public Action RegisterListener(Listener listener)
    {
        listeners.Add(listener);

        return () =>
        {
            logger.LogInformation("Removing a listener");
            listeners.Remove(listener);
        };
    }

    public async Task Dispatch(string value)
    {
        var tasks = listeners.Select(x =>
        {
            logger.LogInformation("Sending event {event} to a listener", value);
            return x.Callback(value);
        });

        await Task.WhenAll(tasks);
    }
}


public record Listener(Func<string, Task> Callback);