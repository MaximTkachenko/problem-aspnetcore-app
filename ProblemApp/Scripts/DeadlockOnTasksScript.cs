namespace ProblemApp.Scripts;

public class DeadlockOnTasksRequest
{
    public long LockTimeoutInMs { get; set; }
    public uint Count { get; set; }
}

/// <summary>
/// Based on https://twitter.com/STeplyakov/status/1449056693568958466
/// </summary>
public class DeadlockOnTasksScript : IStartOnlyScript<DeadlockOnTasksRequest>
{
    private readonly SemaphoreSlim _deadlockOnTasksSemaphore = new SemaphoreSlim(1, 1);
    private readonly List<Task> _tasks = new List<Task>();

    public const string Action = "deadlock-on-tasks";
    public const string Description = "";

    public async Task<bool> StartAsync(DeadlockOnTasksRequest request)
    {
        request.LockTimeoutInMs = request.LockTimeoutInMs == default
            ? 30000
            : request.LockTimeoutInMs;
        request.Count = request.Count == default
            ? 1
            : request.Count;

        await _deadlockOnTasksSemaphore.WaitAsync();

        try
        {
            if (_tasks.Count > 0 && _tasks.Any(t => !t.IsCompleted)) return false;

            _tasks.Clear();

            var tcsStart = new TaskCompletionSource<bool>();
            Task task2 = null;

            for (var i = 0; i < request.Count; i++)
            {
                var task1 = Task.Run(async () =>
                {
                    await tcsStart.Task;

                    // ReSharper disable once AssignNullToNotNullAttribute
                    // ReSharper disable once AccessToModifiedClosure
                    await Task.WhenAny(task2, Task.Delay(TimeSpan.FromMilliseconds(request.LockTimeoutInMs)));
                });
                _tasks.Add(task1);

                task2 = Task.Run(async () =>
                {
                    await tcsStart.Task;

                    await Task.WhenAny(task1, Task.Delay(TimeSpan.FromMilliseconds(request.LockTimeoutInMs)));
                });
                _tasks.Add(task2);
            }

            tcsStart.SetResult(true);
        }
        finally
        {
            _deadlockOnTasksSemaphore.Release();
        }

        return true;
    }
}
