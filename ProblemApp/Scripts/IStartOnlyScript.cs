namespace ProblemApp.Scripts;

// ReSharper disable once TypeParameterCanBeVariant
public interface IStartOnlyScript<TRequestToStart>
{
    Task<bool> StartAsync(TRequestToStart request);
}