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
    public class TaskPollingService : BackgroundService
    {
        private static IEngineClient _engineClient;
        private static IExternalTaskClientHelper _engineClientHelper;
        private static IEnumerable<IExternalTaskExecutor> _taskExecutors;
        private static CamundaSettings _camundaSettings;
        private readonly ILogger _logger;
        private readonly IEnumerable<FetchExternalTaskTopic> _topics;

        public TaskPollingService(IOptions<CamundaSettings> camundaSettings, 
            IEngineClient engineClient,
            IExternalTaskClientHelper engineClientHelper,
            ILogger<TaskPollingService> logger,
            IEnumerable<IExternalTaskExecutor> taskExecutors)
        {
            _engineClient = engineClient;
            _engineClientHelper = engineClientHelper;
            _taskExecutors = taskExecutors;
            _logger = logger;
            _camundaSettings = camundaSettings?.Value;
            _topics = GetTopicsFromExecutors();
        }


        #region PRIVATE METHODS
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var topic in _topics)
                {
                    var fetchLockedExternalTasks = new List<LockedExternalTask>();

                    try
                    {
                        var workerId = Guid.NewGuid().ToString();
                        fetchLockedExternalTasks = await FetchAndLockTasks(workerId, topic);

                        if (fetchLockedExternalTasks.Any())
                        {
                            fetchLockedExternalTasks.ForEach(task => _engineClientHelper.ProcessLockedTasks(workerId, task));
                        }
                    }
                    catch (Exception ex)
                    {
                        await UnlockExternalTask(fetchLockedExternalTasks);
                        _logger.LogError(ex.Message);
                    }
                }

                await Task.Delay(_camundaSettings.PollingIntervalInMilliseconds, stoppingToken);
            }

        }

        private static async Task UnlockExternalTask(IEnumerable<LockedExternalTask> lockedExternalTasks)
        {
            foreach (var externalTask in lockedExternalTasks)
            {
                await _engineClientHelper.UnlockExternalTasks(externalTask.Id);
            }
        }

        private static IEnumerable<FetchExternalTaskTopic> GetTopicsFromExecutors()
        {
            return _taskExecutors.Select(worker => new FetchExternalTaskTopic(
                                        (worker.GetType().GetCustomAttributes(typeof(ExternalTaskTopicAttribute), true).FirstOrDefault() as ExternalTaskTopicAttribute)?.TopicName, 
                                        _camundaSettings.ExternalTaskLockDuration)
            {
                Variables = (worker.GetType().GetCustomAttributes(typeof(ExternalTaskVariableRequirementsAttribute), true).FirstOrDefault() 
                    as ExternalTaskVariableRequirementsAttribute)?.VariablesToFetch

            }).ToList();
        }

        private static Task<List<LockedExternalTask>> FetchAndLockTasks(string workerId, FetchExternalTaskTopic topic)
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
