using System.Threading.Tasks;

namespace ProblemApp.Common
{
    public interface IStartOnlyScript<TRequestToStart>
    {
        string Action { get; }
        string Description { get; }
        Task<bool> StartAsync(TRequestToStart requestToStart);
    }
}