using System.Threading.Tasks;
using Camunda.Api.Client.ExternalTask;

namespace Camunda.Dispatcher.Contracts
{
    public interface IExternalTaskClientHelper
    {
        Task ProcessLockedTasks(string workerId, LockedExternalTask lockedExternalTask);
        Task UnlockExternalTasks(string externalTaskId);
    }
}
