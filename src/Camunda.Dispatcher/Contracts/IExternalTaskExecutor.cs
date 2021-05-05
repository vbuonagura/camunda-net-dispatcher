using System.Collections.Generic;
using System.Threading.Tasks;
using Camunda.Dispatcher.Entities;

namespace Camunda.Dispatcher.Contracts
{
    public interface IExternalTaskExecutor 
    {
         Task<Dictionary<string, object>> Execute(ExternalTask externalTask);
    }
}
