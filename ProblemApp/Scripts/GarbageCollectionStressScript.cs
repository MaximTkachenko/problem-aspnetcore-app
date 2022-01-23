namespace ProblemApp.Scripts;

public class GarbageCollectionStressRequest
{
    public long GarbageCollectionCallPeriodInMs { get; set; }
    public int NumberOfObjects { get; set; }
}

/// <summary>
/// https://www.youtube.com/watch?v=-qtT1wWJi3A
/// https://twitter.com/maoni0/status/1461896972038656009
/// </summary>
public class GarbageCollectionStressScript : IScript<GarbageCollectionStressRequest>
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private Task _allocationTask;
    private Task _gcCollectionTask;
    private CancellationTokenSource _cancellationTokenSource;
    private object[] _objects;

    public const string Action = "gc-stress";
    public const string Description = "";

    public async Task<bool> StartAsync(GarbageCollectionStressRequest request)
    {
        request.GarbageCollectionCallPeriodInMs = request.GarbageCollectionCallPeriodInMs == default
            ? 100
            : request.GarbageCollectionCallPeriodInMs;
        request.NumberOfObjects = request.NumberOfObjects == default
            ? 1024
            : request.NumberOfObjects;

        await _semaphore.WaitAsync();

        try
        {
            if (_objects != null) return false;

            _objects = new object[request.NumberOfObjects];
            _cancellationTokenSource = new CancellationTokenSource();
            _allocationTask = Task.Run(() =>
            {
                var rnd = new Random();
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    int idx = rnd.Next(request.NumberOfObjects);
                    _objects[idx] = new object[rnd.Next(request.NumberOfObjects)];
                }
            }, _cancellationTokenSource.Token);

            _gcCollectionTask = Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(request.GarbageCollectionCallPeriodInMs));
                while(await timer.WaitForNextTickAsync(_cancellationTokenSource.Token))
                {
                    GC.Collect();
                }
            }, _cancellationTokenSource.Token);
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
            if (_objects == null) return false;

            _cancellationTokenSource.Cancel();

            try
            {
                await Task.WhenAll(_allocationTask, _gcCollectionTask);
            }
            catch(OperationCanceledException) { }

            _objects = null;
            _cancellationTokenSource = null;
            _allocationTask = null;
            _gcCollectionTask = null;
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }
}
