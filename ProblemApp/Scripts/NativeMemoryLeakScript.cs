using System.Runtime.InteropServices;

namespace ProblemApp.Scripts;

public class NativeMemoryLeakScriptRequest
{
    public int SizeInBytesPerIteration { get; set; }
    public int PauseInMilliseconds { get; set; }
}

public class NativeMemoryLeakScript : IScript<NativeMemoryLeakScriptRequest>
{
    private readonly List<IntPtr> _handles = new List<IntPtr>();
    private readonly SemaphoreSlim _nativeMemoryLeakSemaphore = new SemaphoreSlim(1, 1);
    private Task _allocationTask;
    private CancellationTokenSource _nativeMemoryLeakTokenSource;

    public const string Action = "native-memory-leak";
    public const string Description = "";

    public async Task<bool> StartAsync(NativeMemoryLeakScriptRequest request)
    {
        request.SizeInBytesPerIteration = request.SizeInBytesPerIteration == default
            ? 10 * 1024 * 1024
            : request.SizeInBytesPerIteration;
        request.PauseInMilliseconds = request.PauseInMilliseconds == default
            ? 5000
            : request.PauseInMilliseconds;

        await _nativeMemoryLeakSemaphore.WaitAsync();

        try
        {
            if (_allocationTask != null) return false;

            _nativeMemoryLeakTokenSource = new CancellationTokenSource();
            _handles.Clear();

            _allocationTask = Task.Run(async () =>
            {
                while (!_nativeMemoryLeakTokenSource.Token.IsCancellationRequested)
                {
                    _handles.Add(Marshal.AllocHGlobal(request.SizeInBytesPerIteration));

                    await Task.Delay(TimeSpan.FromMilliseconds(request.PauseInMilliseconds));
                }
            });
        }
        finally
        {
            _nativeMemoryLeakSemaphore.Release();
        }

        return true;
    }

    public async Task<bool> StopAsync()
    {
        await _nativeMemoryLeakSemaphore.WaitAsync();

        try
        {
            if (_allocationTask == null) return false;

            _nativeMemoryLeakTokenSource.Cancel();
            await _allocationTask;

            foreach (var handle in _handles)
            {
                Marshal.FreeHGlobal(handle);
            }

            _nativeMemoryLeakTokenSource = null;
            _allocationTask = null;
        }
        finally
        {
            _nativeMemoryLeakSemaphore.Release();
        }

        return true;
    }
}
