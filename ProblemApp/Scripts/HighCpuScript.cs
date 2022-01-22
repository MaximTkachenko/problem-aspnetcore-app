namespace ProblemApp.Scripts;

public class HighCpuScriptRequest
{
    public int WorkDurationInMilliseconds { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// https://www.youtube.com/watch?v=_5T4sZHbfoQ
/// https://www.youtube.com/watch?v=e1ZaL2PenTI
/// https://www.youtube.com/watch?v=7llxR-rH-gM
/// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/debug-highcpu?tabs=windows
/// Based on https://github.com/mjrousos/DotNetDiagnosticsSamples/blob/main/3-HighCPU/TargetApp/Services/BadWorker.cs
/// </summary>
public class HighCpuScript : IStartOnlyScript<HighCpuScriptRequest>
{
    public const string Action = "high-cpu";
    public const string Description = "";

    private readonly List<Task> _tasks = new List<Task>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private long _variable;

    public async Task<bool> StartAsync(HighCpuScriptRequest request)
    {
        request.WorkDurationInMilliseconds = request.WorkDurationInMilliseconds == default
            ? 2 * 60 * 1000
            : request.WorkDurationInMilliseconds;
        request.Count = request.Count == default
            ? 25
            : request.Count;

        await _semaphore.WaitAsync();

        try
        {
            if (_tasks.Any(t => !t.IsCompleted)) return false;

            _tasks.Clear();
            Interlocked.Exchange(ref _variable, 0);

            for (int i = 0; i < request.Count; i++)
            {
                _tasks.Add(Task.Run(() =>
                {
                    DoWork(request.WorkDurationInMilliseconds);
                }));
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }

    // Simple compute-bound method useful for testing CPU profilers.
    private void DoWork(int ms)
    {
        if (ms <= 0)
        {
            return;
        }

        ms -= 50;

        SpinFor50Ms();

        WorkHelper(ms);
    }

    // WorkHelper is a clone of DoWork that is included
    // to make for more interesting call stacks.
    private void WorkHelper(int ms)
    {
        if (ms <= 0)
        {
            return;
        }

        ms -= 50;

        SpinFor50Ms();

        DoWork(ms);
    }

    // SpinFor50ms repeatedly calls DateTime.Now until for
    // 50ms.  It also does some work of its own in this
    // methods so we get some exclusive time to look at.  
    private void SpinFor50Ms()
    {
        DateTime start = DateTime.Now;
        for (; ; )
        {
            if ((DateTime.Now - start).TotalMilliseconds > 50)
                break;

            // Do some work in this routine as well.   
            for (int i = 0; i < 10; i++)
            {
                Interlocked.Add(ref _variable, i);
            }
        }
    }
}