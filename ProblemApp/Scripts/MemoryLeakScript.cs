namespace ProblemApp.Scripts;

public sealed class MemoryLeakScriptRequest
{

}

/// <summary>
/// https://www.youtube.com/watch?v=SHGeE_PFA4s
/// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/debug-memory-leak
/// </summary>
public class MemoryLeakScript : IScript<MemoryLeakScriptRequest>
{
    private readonly SemaphoreSlim _mlSemaphore = new SemaphoreSlim(1, 1);

    public Task<bool> StartAsync(MemoryLeakScriptRequest requestToStart)
    {

        throw new NotImplementedException();
    }

    public Task<bool> StopAsync()
    {
        throw new NotImplementedException();
    }
}