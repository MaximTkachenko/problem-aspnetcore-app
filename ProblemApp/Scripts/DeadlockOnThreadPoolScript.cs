namespace ProblemApp.Scripts;

public class DeadlockOnThreadPoolRequest
{
    public long LockTimeoutInMs { get; set; }
    public uint Count { get; set; }
}

/// <summary>
/// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/debug-deadlock?tabs=windows
/// </summary>
public class DeadlockOnThreadPoolScript : IStartOnlyScript<DeadlockOnThreadPoolRequest>
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly object _lockB = new object();
    private readonly object _lockA = new object();
    private readonly List<Task> _tasks = new List<Task>();

    public const string Action = "deadlock-on-threadpool";
    public const string Description = "";

    public async Task<bool> StartAsync(DeadlockOnThreadPoolRequest request)
    {
        request.LockTimeoutInMs = request.LockTimeoutInMs == default
            ? 30000
            : request.LockTimeoutInMs;
        request.Count = request.Count == default
            ? 1
            : request.Count;

        await _semaphore.WaitAsync();

        try
        {
            if (_tasks.Count > 0 && _tasks.Any(t => !t.IsCompleted)) return false;

            _tasks.Clear();

            for (var i = 0; i < request.Count; i++)
            {
                _tasks.Add(Task.Run(() =>
                {
                    if (Monitor.TryEnter(_lockA))
                    {
                        Thread.Sleep(100);

                        try
                        {
                            if (Monitor.TryEnter(_lockB, TimeSpan.FromMilliseconds(request.LockTimeoutInMs)))
                            {
                                try
                                {
                                    //actually nothing here
                                }
                                finally
                                {
                                    Monitor.Exit(_lockB);
                                }
                            }
                        }
                        finally
                        {
                            Monitor.Exit(_lockA);
                        }
                    }
                }));

                _tasks.Add(Task.Run(() =>
                {
                    if (Monitor.TryEnter(_lockB))
                    {
                        try
                        {
                            if (Monitor.TryEnter(_lockA, TimeSpan.FromMilliseconds(request.LockTimeoutInMs)))
                            {
                                try
                                {
                                    //actually nothing here
                                }
                                finally
                                {
                                    Monitor.Exit(_lockA);
                                }
                            }
                        }
                        finally
                        {
                            Monitor.Exit(_lockB);
                        }
                    }
                }));
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }
}