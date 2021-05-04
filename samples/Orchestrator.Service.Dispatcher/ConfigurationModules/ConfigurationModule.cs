using Camunda.Dispatcher;
using Camunda.Dispatcher.Contracts;
using Camunda.Dispatcher.Core;
using Camunda.Dispatcher.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleWebApi.Dispatchers;

namespace SampleWebApi.ConfigurationModules
{
    /// <summary>
    /// Configuration class to add  configurations
    /// </summary>
    public static class ConfigurationModule
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddTransient<IEngineClient, EngineClient>()
                .AddTransient<IExternalTaskClientHelper, ExternalTaskClientHelper>()

                //Instantiate external task executors
                .AddTransient<IExternalTaskExecutor, UpdateProcessStatus>()
                .AddTransient<IExternalTaskExecutor, RegisterAttribute>()

                .Configure<CamundaSettings>(configuration.GetSection("CamundaSettings"))
                .Configure<ProxySettings>(configuration.GetSection("ProxySettings"))

                .AddHostedService<TaskPollingService>();

        }
    }
}
