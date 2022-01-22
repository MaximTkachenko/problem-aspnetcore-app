namespace ProblemApp.Scripts;

public class ThreadPoolStarvationWithHttpCallScriptRequest
{
    public int DelayInMilliseconds { get; set; }
    public int OperationsPerIteration { get; set; }
}

/// <summary>
/// https://www.youtube.com/watch?v=isK8Cel3HP0
/// https://labs.criteo.com/2018/10/net-threadpool-starvation-and-how-queuing-makes-it-worse/
/// </summary>
public class ThreadPoolStarvationWithHttpCallScript : IScript<ThreadPoolStarvationWithHttpCallScriptRequest>
{
    private readonly List<Task> _tasks = new List<Task>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private Task _workloadTask;
    private CancellationTokenSource _cancellationTokenSource;

    public const string Action = "threadpool-starvation-with-http-call";
    public const string Description = "";

    private readonly IHttpClientFactory _http;

    public ThreadPoolStarvationWithHttpCallScript(IHttpClientFactory http)
    {
        _http = http;
    }

    public async Task<bool> StartAsync(ThreadPoolStarvationWithHttpCallScriptRequest request)
    {
        request.DelayInMilliseconds = request.DelayInMilliseconds == default
            ? 5000
            : request.DelayInMilliseconds;
        request.OperationsPerIteration = request.OperationsPerIteration == default
            ? 20
            : request.OperationsPerIteration;

        await _semaphore.WaitAsync();

        try
        {
            if (_workloadTask != null) return false;

            _cancellationTokenSource = new CancellationTokenSource();

            _workloadTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    for (int i = 0; i < request.OperationsPerIteration; i++)
                    {
                        _tasks.Add(Task.Run(() =>
                        {
                            _http.CreateClient().GetStringAsync(@$"https://deelay.me/{request.DelayInMilliseconds}/https://mtkachenko.me", _cancellationTokenSource.Token)
                                .Wait(_cancellationTokenSource.Token);
                        }, _cancellationTokenSource.Token));
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }

    public async Task<bool> StopAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_workloadTask == null) return false;

            _cancellationTokenSource.Cancel();
            await _workloadTask;
            try
            {
                await Task.WhenAll(_tasks);
            }
            catch (OperationCanceledException) { }
            
            _cancellationTokenSource = null;
            _workloadTask = null;
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }
}