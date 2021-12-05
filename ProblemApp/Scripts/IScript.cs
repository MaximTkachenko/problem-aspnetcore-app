namespace ProblemApp.Scripts;

public interface IScript<TRequestToStart>
{
    Task<bool> StartAsync(TRequestToStart requestToStart);
    Task<bool> StopAsync();
}