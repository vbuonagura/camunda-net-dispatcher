using Camunda.Api.Client;

namespace Camunda.Dispatcher.Contracts
{
    public interface IEngineClient
    {
        CamundaClient Client();
    }
}
