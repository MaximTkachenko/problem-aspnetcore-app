namespace ProblemApp.Scripts;

public class DeadlockedWithThreadsRequest
{
    public long LockTimeoutInMs { get; set; }
}

/// <summary>
/// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/debug-deadlock?tabs=windows
/// </summary>
public class DeadlockedWithThreadsScript : IStartOnlyScript<DeadlockedWithThreadsRequest>
{
    private static readonly Lazy<DeadlockedWithThreadsScript> LazyInstance =
        new Lazy<DeadlockedWithThreadsScript>(() => new DeadlockedWithThreadsScript());

    private DeadlockedWithThreadsScript() { }

    public static DeadlockedWithThreadsScript Instance => LazyInstance.Value;

    public Task<bool> StartAsync(DeadlockedWithThreadsRequest request)
    {
        request.LockTimeoutInMs = request.LockTimeoutInMs == default
            ? 30000
            : request.LockTimeoutInMs;

        var lockB = new object();
        var lockA = new object();
        Thread thread = new Thread(() =>
        {
            if (Monitor.TryEnter(lockA))
            {
                Thread.Sleep(100);

                try
                {
                    if (Monitor.TryEnter(lockB, TimeSpan.FromMilliseconds(request.LockTimeoutInMs)))
                    {
                        try
                        {
                            //actually nothing here
                        }
                        finally
                        {
                            Monitor.Exit(lockB);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(lockA);
                }
            }
        });
        thread.Name = $"thread 1: locks A then B";
        thread.Start();

        thread = new Thread(() =>
        {
            if (Monitor.TryEnter(lockB))
            {
                try
                {
                    if (Monitor.TryEnter(lockA, TimeSpan.FromMilliseconds(request.LockTimeoutInMs)))
                    {
                        try
                        {
                            //actually nothing here
                        }
                        finally
                        {
                            Monitor.Exit(lockA);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(lockB);
                }
            }
        });
        thread.Name = $"thread 2: locks B then A";
        thread.Start();

        return Task.FromResult(true);
    }
}
