using System.Collections.Concurrent;

namespace ProblemApp.Scripts;

public class DeadlockOnThreadPoolRequest
{
    public long LockTimeoutInMs { get; set; }
    public uint Count { get; set; }
}

public class DeadlockOnThreadPoolScript : IStartOnlyScript<DeadlockOnThreadPoolRequest>
{
    private readonly object _lockB = new object();
    private readonly object _lockA = new object();
    private ConcurrentBag<Task> _tasks = new ConcurrentBag<Task>();

    public const string Action = "deadlock-on-threadpool";

    public Task<bool> StartAsync(DeadlockOnThreadPoolRequest request)
    {
        request.LockTimeoutInMs = request.LockTimeoutInMs == default
            ? 30000
            : request.LockTimeoutInMs;
        request.Count = request.Count == default
            ? 1
            : request.Count;

        for (var i = 0; i < request.Count; i++)
        {
            var task1 = Task.Run(() =>
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

            var task2 = Task.Run(() =>
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

            _tasks.Add(task1);
            _tasks.Add(task2);
        }

        return Task.FromResult(true);
    }
}