namespace ProblemApp.Scripts;

public interface IScript<TRequestToStart> : IStartOnlyScript<TRequestToStart>
{
    Task<bool> StopAsync();
}
