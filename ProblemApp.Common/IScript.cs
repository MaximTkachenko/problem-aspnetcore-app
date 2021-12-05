using System.Threading.Tasks;

namespace ProblemApp.Common
{
    public interface IScript<TRequestToStart>
    {
        Task<bool> StartAsync(TRequestToStart requestToStart);
        Task<bool> StopAsync();
    }
}
