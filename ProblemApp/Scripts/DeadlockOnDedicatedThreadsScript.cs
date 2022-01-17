namespace ProblemApp.Scripts;

public class DeadlockOnDedicatedThreadsRequest
{
    public long LockTimeoutInMs { get; set; }
    public uint Count { get; set; }
}

/// <summary>
/// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/debug-deadlock?tabs=windows
/// </summary>
public class DeadlockOnDedicatedThreadsScript : IStartOnlyScript<DeadlockOnDedicatedThreadsRequest>
{
    private readonly SemaphoreSlim _deadlockOnDedicatedThreadsSemaphore = new SemaphoreSlim(1, 1);
    private readonly object _lockB = new object();
    private readonly object _lockA = new object();
    private readonly List<Thread> _threads = new List<Thread>();

    public const string Action = "deadlock-on-dedicated-threads";
    public const string Description = "";

    public async Task<bool> StartAsync(DeadlockOnDedicatedThreadsRequest request)
    {
        request.LockTimeoutInMs = request.LockTimeoutInMs == default
            ? 30000
            : request.LockTimeoutInMs;
        request.Count = request.Count == default
            ? 1
            : request.Count;

        await _deadlockOnDedicatedThreadsSemaphore.WaitAsync();

        try
        {
            if (_threads.Count > 0 && _threads.Any(th => th.ThreadState != ThreadState.Stopped)) return false;

            _threads.Clear();
            var threadNamePrefix = Guid.NewGuid().ToString();

            for (var i = 0; i < request.Count; i++)
            {
                var thread1 = new Thread(() =>
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
                });
                thread1.Name = $"[{threadNamePrefix}#{i}] thread 1: locks A then B";
                thread1.Start();
                _threads.Add(thread1);

                var thread2 = new Thread(() =>
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
                });
                thread2.Name = $"[{threadNamePrefix}#{i}] thread 2: locks B then A";
                thread2.Start();
                _threads.Add(thread2);
            }
        }
        finally
        {
            _deadlockOnDedicatedThreadsSemaphore.Release();
        }

        return true;
    }
}
