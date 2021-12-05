using System.Threading.Tasks;

namespace ProblemApp.Common
{
    public interface IScript<TRequestToStart> : IStartOnlyScript<TRequestToStart>
    {
        Task<bool> StopAsync();
    }
}
