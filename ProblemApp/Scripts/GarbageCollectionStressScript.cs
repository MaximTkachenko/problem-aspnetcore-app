namespace ProblemApp.Scripts;

/// <summary>
/// https://www.youtube.com/watch?v=-qtT1wWJi3A
/// </summary>
public class GarbageCollectionStressScript
{
    public class StartGarbageCollectionStressRequest
    {
        public long GarbafeCollectionCallPeriodInMs { get; set; }
        public int NumberOfObjects { get; set; }
    }

    private static readonly Lazy<GarbageCollectionStressScript> LazyInstance =
        new Lazy<GarbageCollectionStressScript>(() => new GarbageCollectionStressScript());

    private readonly SemaphoreSlim _gcStressSemaphore = new SemaphoreSlim(1, 1);
    private Task _allocationTask;
    private Task _gcCollectionTask;
    private CancellationTokenSource _gcStressCancellationTokenSource;
    private object[] _objects;

    private GarbageCollectionStressScript() { }

    public static GarbageCollectionStressScript Instance => LazyInstance.Value;

    public async Task<bool> StartAsync(StartGarbageCollectionStressRequest request)
    {
        request.GarbafeCollectionCallPeriodInMs = request.GarbafeCollectionCallPeriodInMs == default
            ? 100
            : request.GarbafeCollectionCallPeriodInMs;
        request.NumberOfObjects = request.NumberOfObjects == default
            ? 1024
            : request.NumberOfObjects;

        await _gcStressSemaphore.WaitAsync();

        try
        {
            if (_objects != null) return false;

            _objects = new object[request.NumberOfObjects];
            _gcStressCancellationTokenSource = new CancellationTokenSource();
            _allocationTask = Task.Run(() =>
            {
                var rnd = new Random();
                while (!_gcStressCancellationTokenSource.Token.IsCancellationRequested)
                {
                    int idx = rnd.Next(request.NumberOfObjects);
                    _objects[idx] = new object[rnd.Next(request.NumberOfObjects)];
                }
            }, _gcStressCancellationTokenSource.Token);

            _gcCollectionTask = Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(request.GarbafeCollectionCallPeriodInMs));
                while(await timer.WaitForNextTickAsync(_gcStressCancellationTokenSource.Token))
                {
                    GC.Collect();
                }
            }, _gcStressCancellationTokenSource.Token);
        }
        finally
        {
            _gcStressSemaphore.Release();
        }

        return true;
    }

    public async Task<bool> StopAsync()
    {
        await _gcStressSemaphore.WaitAsync();
        try
        {
            if (_objects == null) return false;

            _gcStressCancellationTokenSource.Cancel();

            try
            {
                await Task.WhenAll(_allocationTask, _gcCollectionTask);
            }
            catch(OperationCanceledException) { }

            _objects = null;
            _gcStressCancellationTokenSource = null;
            _allocationTask = null;
            _gcCollectionTask = null;
        }
        finally
        {
            _gcStressSemaphore.Release();
        }

        return true;
    }
}
