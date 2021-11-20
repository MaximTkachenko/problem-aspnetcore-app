namespace ProblemApp.Scripts;

public interface IStartOnlyScript<TRequestToStart>
{
    Task<bool> StartAsync(TRequestToStart requestToStart);
}