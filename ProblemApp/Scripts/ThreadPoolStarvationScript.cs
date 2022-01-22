namespace ProblemApp.Scripts;

public class ThreadPoolStarvationScriptRequest
{
    public int DelayInMilliseconds { get; set; }
    public int OperationsPerIteration { get; set; }
}

/// <summary>
/// https://www.youtube.com/watch?v=isK8Cel3HP0
/// https://labs.criteo.com/2018/10/net-threadpool-starvation-and-how-queuing-makes-it-worse/
/// </summary>
public class ThreadPoolStarvationScript : IScript<ThreadPoolStarvationScriptRequest>
{
    private readonly List<Task> _tasks = new List<Task>();
    private readonly SemaphoreSlim _threadPoolStarvationSemaphore = new SemaphoreSlim(1, 1);
    private Task _workloadTask;
    private CancellationTokenSource _threadPoolStarvationTokenSource;

    public const string Action = "threadpool-starvation";
    public const string Description = "";

    private readonly IHttpClientFactory _http;

    public ThreadPoolStarvationScript(IHttpClientFactory http)
    {
        _http = http;
    }

    public async Task<bool> StartAsync(ThreadPoolStarvationScriptRequest request)
    {
        request.DelayInMilliseconds = request.DelayInMilliseconds == default
            ? 5000
            : request.DelayInMilliseconds;
        request.OperationsPerIteration = request.OperationsPerIteration == default
            ? 20
            : request.OperationsPerIteration;

        await _threadPoolStarvationSemaphore.WaitAsync();

        try
        {
            if (_workloadTask != null) return false;

            _threadPoolStarvationTokenSource = new CancellationTokenSource();

            _workloadTask = Task.Run(async () =>
            {
                while (!_threadPoolStarvationTokenSource.Token.IsCancellationRequested)
                {
                    for (int i = 0; i < request.OperationsPerIteration; i++)
                    {
                        _tasks.Add(Task.Run(() =>
                        {
                            _http.CreateClient().GetStringAsync(@$"https://deelay.me/{request.DelayInMilliseconds}/https://mtkachenko.me", _threadPoolStarvationTokenSource.Token)
                                .Wait(_threadPoolStarvationTokenSource.Token);
                        }, _threadPoolStarvationTokenSource.Token));
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
        }
        finally
        {
            _threadPoolStarvationSemaphore.Release();
        }

        return true;
    }

    public async Task<bool> StopAsync()
    {
        await _threadPoolStarvationSemaphore.WaitAsync();

        try
        {
            if (_workloadTask == null) return false;

            _threadPoolStarvationTokenSource.Cancel();
            await _workloadTask;
            try
            {
                await Task.WhenAll(_tasks);
            }
            catch (OperationCanceledException) { }
            
            _threadPoolStarvationTokenSource = null;
            _workloadTask = null;
        }
        finally
        {
            _threadPoolStarvationSemaphore.Release();
        }

        return true;
    }
}