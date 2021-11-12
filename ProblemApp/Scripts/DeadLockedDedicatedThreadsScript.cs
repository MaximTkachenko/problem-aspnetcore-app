namespace ProblemApp.Scripts;

/// <summary>
/// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/debug-deadlock?tabs=windows
/// </summary>
public class DeadLockedDedicatedThreadsScript
{
    public void Start()
    {
        string lockB = "Lock_A";
        string lockA = "Lock_B";
        Thread thread = new Thread(() =>
        {
            lock (lockA)
            {
                Thread.Sleep(100);
                lock (lockB)
                {
                    while (true)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        });
        thread.Name = $"thread 1: locks A then B";
        thread.Start();

        thread = new Thread(() =>
        {
            lock (lockB)
            {
                lock (lockA)
                {
                    while (true)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        });
        thread.Name = $"thread 2: locks B then A";
        thread.Start();
    }
}
