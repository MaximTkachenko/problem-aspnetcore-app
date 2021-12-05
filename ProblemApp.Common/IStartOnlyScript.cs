using System.Threading.Tasks;

namespace ProblemApp.Common
{
    public interface IStartOnlyScript<TRequestToStart>
    {
        Task<bool> StartAsync(TRequestToStart requestToStart);
    }
}