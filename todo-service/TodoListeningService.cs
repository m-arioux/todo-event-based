namespace todo_service;

public class TodoListeningService(ILogger<TodoListeningService> logger)
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

    public async Task Dispatch(Todo todo)
    {
        var tasks = listeners.Select(x =>
        {
            logger.LogInformation("Sending todo {todo} to a listener", todo);
            return x.Callback(todo);
        });

        await Task.WhenAll(tasks);
    }
}


public record Listener(Func<Todo, Task> Callback);