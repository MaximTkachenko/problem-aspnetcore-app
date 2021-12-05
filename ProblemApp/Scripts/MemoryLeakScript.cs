using ProblemApp.Common;

namespace ProblemApp.Scripts;

public sealed class MemoryLeakScriptRequest
{
    public int NumberOfObjects { get; set; }
}

/// <summary>
/// https://www.youtube.com/watch?v=SHGeE_PFA4s
/// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/debug-memory-leak
/// </summary>
public class MemoryLeakScript : IScript<MemoryLeakScriptRequest>
{
    private readonly SemaphoreSlim _mlSemaphore = new SemaphoreSlim(1, 1);
    private List<object[]> _objects;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _leakTask;

    public const string Action = "memory-leak";

    public async Task<bool> StartAsync(MemoryLeakScriptRequest requestToStart)
    {
        requestToStart.NumberOfObjects = requestToStart.NumberOfObjects == default
               ? 1024
               : requestToStart.NumberOfObjects;

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
                    int idx = rnd.Next(requestToStart.NumberOfObjects);
                    _objects.Add(new object[rnd.Next(requestToStart.NumberOfObjects)]);
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