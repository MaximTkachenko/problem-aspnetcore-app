namespace ProblemApp.Scripts;

public class DeadlockedWithTasksRequest
{
    public long LockTimeoutInMs { get; set; }
}

public class DeadlockedWithTasksScript : IStartOnlyScript<DeadlockedWithTasksRequest>
{
    private static readonly Lazy<DeadlockedWithTasksScript> LazyInstance =
        new Lazy<DeadlockedWithTasksScript>(() => new DeadlockedWithTasksScript());

    private DeadlockedWithTasksScript() { }

    public static DeadlockedWithTasksScript Instance => LazyInstance.Value;

    public Task<bool> StartAsync(DeadlockedWithTasksRequest request)
    {
        request.LockTimeoutInMs = request.LockTimeoutInMs == default
            ? 30000
            : request.LockTimeoutInMs;

        var lockB = new object();
        var lockA = new object();
        Task.Run(() =>
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

        Task.Run(() =>
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

        return Task.FromResult(true);
    }
}