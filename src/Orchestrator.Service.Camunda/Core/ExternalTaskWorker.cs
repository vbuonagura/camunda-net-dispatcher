using Camunda.Api.Client;
using Camunda.Api.Client.ExternalTask;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Review.Process.Camunda.Core
{
    public class ExternalTaskWorker : IDisposable
    {
        private readonly string _workerId = Guid.NewGuid().ToString();

        private Timer _timer;

        private readonly CamundaClient _camundaClient;
        private readonly ExternalTaskWorkerInfo _externalTaskWorkerInfo;
        private readonly FetchExternalTasks _fetching;

        private readonly long _pollingInterval;
        private readonly int _maxDegreeOfParallelism;


        public ExternalTaskWorker(
            CamundaClient camundaClient,
            ExternalTaskWorkerInfo externalTaskWorkerInfo,
            WorkerSettings workerSettings)
        {
            _camundaClient = camundaClient;
            _externalTaskWorkerInfo = externalTaskWorkerInfo;

            _pollingInterval = workerSettings.ExternalTaskSettings.PollingInterval;
            var lockDuration = workerSettings.ExternalTaskSettings.LockDuration;
            _maxDegreeOfParallelism = workerSettings.ExternalTaskSettings.MaxDegreeOfParallelism;
            var maxTasksToFetchAtOnce = workerSettings.ExternalTaskSettings.MaxTasksToFetchAtOnce;

            var topic = new FetchExternalTaskTopic(_externalTaskWorkerInfo.TopicName, lockDuration)
            {
                Variables = _externalTaskWorkerInfo.VariablesToFetch
            };

            _fetching = new FetchExternalTasks()
            {
                WorkerId = _workerId,
                MaxTasks = maxTasksToFetchAtOnce,
                UsePriority = true,
                Topics = new List<FetchExternalTaskTopic>() { topic }
            };
        }

        public async void DoPolling()
        {
            try
            {
                var lockedExternalTasks = await _camundaClient.ExternalTasks.FetchAndLock(_fetching);

                // run them in parallel with a max degree of parallelism
                Parallel.ForEach(
                    lockedExternalTasks,
                    new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
                    lockedExternalTask => Execute(lockedExternalTask, _externalTaskWorkerInfo.Type)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            _timer.Change(_pollingInterval, Timeout.Infinite);
        }

        private void CreateAdapterInstanceAndExecute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables, Type taskType)
        {
            var constructor = taskType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            if (constructor.Length > 0)
            {

                List<object> constructorInstance = new List<object>();
                var classConstructor = constructor[0];
                var parameters = classConstructor.GetParameters();

                using (var serviceScope = ServiceLocator.Instance.GetService<IServiceScopeFactory>().CreateScope())
                {
                    foreach (ParameterInfo parameter in parameters)
                    {
                        var instance = serviceScope.ServiceProvider.GetRequiredService(parameter.ParameterType);
                        constructorInstance.Add(instance);
                    }

                    if (constructorInstance.Any())
                    {
                        var worker = classConstructor.Invoke(constructorInstance.ToArray()) as IExternalTaskAdapter;

                        worker.Execute(externalTask, ref resultVariables);
                    }
                    else
                    {
                        if (Activator.CreateInstance(taskType) is IExternalTaskAdapter worker)
                            worker.Execute(externalTask, ref resultVariables);
                        else
                        {
                            var errMessage = $"Constructor not found on type:{taskType.FullName}";
                            LogEventToConsole(errMessage, "ERROR");
                            throw new MethodAccessException(errMessage);
                        }
                    }
                }
            }
        }

        private void Execute(LockedExternalTask lockedExternalTask, Type type)
        {
            ExternalTask externalTask = lockedExternalTask.ToExternalTask();
            var resultVariables = new Dictionary<string, object>();

            try
            {
                LogEventToConsole($"{externalTask.ProcessDefinitionId} {externalTask.TopicName} Started", "INFO");

                CreateAdapterInstanceAndExecute(externalTask, ref resultVariables, type);

                LogEventToConsole($"{externalTask.ProcessDefinitionId} {externalTask.TopicName} Finished", "INFO");

                CompleteExternalTask completeExternalTask = new CompleteExternalTask()
                {
                    WorkerId = _workerId,
                    Variables = resultVariables.ToVariableDictionary()
                };
                _camundaClient.ExternalTasks[externalTask.Id].Complete(completeExternalTask);
            }
            catch (ExternalTaskException ex)
            {
                LogEventToConsole($"{externalTask.ProcessDefinitionId} {externalTask.TopicName} {ex.Message}", "ERROR");

                ExternalTaskBpmnError externalTaskBpmnError = new ExternalTaskBpmnError()
                {
                    WorkerId = _workerId,
                    ErrorCode = ex.BusinessErrorCode
                };
                _camundaClient.ExternalTasks[externalTask.Id].HandleBpmnError(externalTaskBpmnError);
            }
            catch (Exception ex)
            {
                LogEventToConsole($"{externalTask.ProcessDefinitionId} {externalTask.TopicName} {ex.Message}", "ERROR");

                var retriesLeft = _externalTaskWorkerInfo.Retries; // start with default
                if (externalTask.Retries.HasValue) // or decrement if retries are already set
                {
                    retriesLeft = externalTask.Retries.Value - 1;
                }

                ExternalTaskFailure externalTaskFailure = new ExternalTaskFailure()
                {
                    WorkerId = _workerId,
                    ErrorMessage = ex.Message,
                    ErrorDetails = ex.StackTrace,
                    RetryTimeout = _externalTaskWorkerInfo.RetryTimeout,
                    Retries = retriesLeft
                };

                _camundaClient.ExternalTasks[externalTask.Id].HandleFailure(externalTaskFailure);
            }
        }

        public void StartWork()
        {
            _timer = new Timer(_ => DoPolling(), null, _pollingInterval, Timeout.Infinite);
        }

        public void StopWork()
        {
            _timer.Dispose();
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
        private void LogEventToConsole(string message, string level)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")} {level} Review.Process.Service {message}");
        }
    }

    public static class ServiceLocator
    {
        public static IServiceProvider Instance { get; set; }
    }

}
