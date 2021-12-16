namespace ProblemApp.Scripts;

public interface IScript<TRequestToStart>
{
    Task<bool> StartAsync(TRequestToStart request);
    Task<bool> StopAsync();
}