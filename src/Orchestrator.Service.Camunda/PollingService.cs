using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Camunda.Api.Client.ExternalTask;
using Camunda.Dispatcher.Contracts;
using Camunda.Dispatcher.Core;
using Camunda.Dispatcher.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Camunda.Dispatcher
{
    /// <summary>
    /// This helper will process all pending activities from camunda.
    /// </summary>
    public class PollingService : BackgroundService
    {
        private static IEngineClient _engineClient;
        private static IExternalTaskClientHelper _engineClientHelper;
        private static IEnumerable<IExternalTaskExecutor> _taskExecutors;
        private static ILogger _logger;
        private static CamundaSettings _camundaSettings;

        public PollingService(IOptions<CamundaSettings> camundaSettings, 
            IEngineClient engineClient,
            IExternalTaskClientHelper engineClientHelper,
            ILogger logger,
            IEnumerable<IExternalTaskExecutor> taskExecutors)
        {
            _engineClient = engineClient;
            _engineClientHelper = engineClientHelper;
            _logger = logger;
            _taskExecutors = taskExecutors;
            _camundaSettings = camundaSettings?.Value;
        }


        #region PRIVATE METHODS
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var topics = GetTopics();

                foreach (var topic in topics)
                {
                    var fetchLockedExternalTasks = new List<LockedExternalTask>();
                    try
                    {
                        var workerId = Guid.NewGuid().ToString();
                        fetchLockedExternalTasks = await FetchTask(workerId, topic);

                        if (fetchLockedExternalTasks.Any())
                        {
                            fetchLockedExternalTasks.ForEach(task => _engineClientHelper.ProcessLockedTasks(workerId, task));
                        }
                    }
                    catch (Exception ex)
                    {
                        await UnlockExternalTask(fetchLockedExternalTasks);
                        _logger?.LogError(ex.Message);
                    }
                }

                await Task.Delay(_camundaSettings.PollingIntervalInMilliseconds, stoppingToken);
            }

        }

        private static async Task UnlockExternalTask(List<LockedExternalTask> lockedExternalTasks)
        {
            foreach (var externalTask in lockedExternalTasks)
            {
                await _engineClientHelper.UnlockExternalTasks(externalTask.Id);
            }
        }

        private static IEnumerable<FetchExternalTaskTopic> GetTopics()
        {
            return _taskExecutors.Select(worker => new FetchExternalTaskTopic(
                                        (worker.GetType().GetCustomAttributes(typeof(ExternalTaskTopicAttribute), true).FirstOrDefault() as ExternalTaskTopicAttribute)?.TopicName, 
                                        _camundaSettings.ExternalTaskLockDuration)
            {
                Variables = (worker.GetType().GetCustomAttributes(typeof(ExternalTaskVariableRequirementsAttribute), true).FirstOrDefault() 
                    as ExternalTaskVariableRequirementsAttribute)?.VariablesToFetch

            }).ToList();
        }

        private static Task<List<LockedExternalTask>> FetchTask(string workerId, FetchExternalTaskTopic topic)
        {
            var fetchingQuery = new FetchExternalTasks
            {
                WorkerId = workerId,
                MaxTasks = _camundaSettings.FetchTaskCount,
                UsePriority = false,
                Topics = new List<FetchExternalTaskTopic> { topic }
            };

            return _engineClient.Client().ExternalTasks.FetchAndLock(fetchingQuery);
        }

        #endregion
    }
}
