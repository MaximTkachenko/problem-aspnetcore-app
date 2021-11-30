namespace ProblemApp.Scripts;

public class DeadlockedWithThreadsRequest
{
    public long LockTimeoutInMs { get; set; }
    public uint Count { get; set; }
    public string ThreadNamePrefix { get; set; }
}

/// <summary>
/// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/debug-deadlock?tabs=windows
/// </summary>
public class DeadlockedWithThreadsScript : IStartOnlyScript<DeadlockedWithThreadsRequest>
{
    private readonly object _lockB = new object();
    private readonly object _lockA = new object();

    public Task<bool> StartAsync(DeadlockedWithThreadsRequest request)
    {
        request.LockTimeoutInMs = request.LockTimeoutInMs == default
            ? 30000
            : request.LockTimeoutInMs;
        request.Count = request.Count == default
            ? 1
            : request.Count;

        for (var i = 0; i < request.Count; i++)
        {
            Thread thread = new Thread(() =>
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
            thread.Name = $"[{request.ThreadNamePrefix}] thread 1: locks A then B";
            thread.Start();

            thread = new Thread(() =>
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
            thread.Name = $"[{request.ThreadNamePrefix}] thread 2: locks B then A";
            thread.Start();
        }

        return Task.FromResult(true);
    }
}
