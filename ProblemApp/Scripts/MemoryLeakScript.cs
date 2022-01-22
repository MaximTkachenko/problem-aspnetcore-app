namespace ProblemApp.Scripts;

public sealed class MemoryLeakScriptRequest
{
    public int NumberOfObjects { get; set; }
}

/// <summary>
/// https://www.youtube.com/watch?v=SHGeE_PFA4s
/// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/debug-memory-leak
/// https://www.tessferrandez.com/blog/2021/03/18/debugging-a-netcore-memory-issue-with-dotnet-dump.html
/// </summary>
public class MemoryLeakScript : IScript<MemoryLeakScriptRequest>
{
    private readonly SemaphoreSlim _mlSemaphore = new SemaphoreSlim(1, 1);
    private List<object[]> _objects;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _leakTask;

    public const string Action = "memory-leak";
    public const string Description = "";

    public async Task<bool> StartAsync(MemoryLeakScriptRequest request)
    {
        request.NumberOfObjects = request.NumberOfObjects == default
               ? 1024
               : request.NumberOfObjects;

        await _mlSemaphore.WaitAsync();

        try
        {
            if (_objects != null) return false;

            _objects = new List<object[]>();
            _cancellationTokenSource = new CancellationTokenSource();
            _leakTask = Task.Run(() =>
            {
                var rnd = new Random();
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    int idx = rnd.Next(request.NumberOfObjects);
                    _objects.Add(new object[rnd.Next(request.NumberOfObjects)]);
                }
            }, _cancellationTokenSource.Token);
        }
        finally
        {
            _mlSemaphore.Release();
        }

        return true;
    }

    public async Task<bool> StopAsync()
    {
        await _mlSemaphore.WaitAsync();
        try
        {
            if (_objects == null) return false;

            _cancellationTokenSource.Cancel();

            try
            {
                await _leakTask;
            }
            catch (OperationCanceledException) { }

            _objects = null;
            _cancellationTokenSource = null;
        }
        finally
        {
            _mlSemaphore.Release();
        }

        return true;
    }
}