namespace ProblemApp.Scripts;

// ReSharper disable once TypeParameterCanBeVariant
public interface IScript<TRequestToStart>
{
    Task<bool> StartAsync(TRequestToStart request);
    Task<bool> StopAsync();
}