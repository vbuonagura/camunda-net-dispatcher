using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Camunda.Api.Client.ExternalTask;
using Camunda.Dispatcher.Contracts;
using Camunda.Dispatcher.Entities;
using Camunda.Dispatcher.Exceptions;
using Camunda.Dispatcher.Helpers;

namespace Camunda.Dispatcher.Core
{
    public class ExternalTaskClientHelper : IExternalTaskClientHelper
    {
        private readonly IEngineClient _engineClient;
        private readonly IEnumerable<IExternalTaskExecutor> _externalTaskExecutors;

        public ExternalTaskClientHelper(IEngineClient engineClient, IEnumerable<IExternalTaskExecutor> externalTaskExecutors)
        {
            _engineClient = engineClient;
            _externalTaskExecutors = externalTaskExecutors;
        }

        #region PUBLIC METHODS

        public async Task ProcessLockedTasks(string workerId, LockedExternalTask lockedExternalTask)
        {
            var executor = GetExecutor(lockedExternalTask.TopicName);
            var executorAttribute = GetExecutorAttributeData(executor);
            var externalTask = lockedExternalTask.ToExternalTask();

            try
            {
                var resultVariables = await ExecuteExternalTask(executor, externalTask);

                var completeExternalTask = new CompleteExternalTask
                {
                    WorkerId = workerId,
                    Variables = resultVariables.ToVariableDictionary()
                };

                await _engineClient.Client().ExternalTasks[externalTask.Id].Complete(completeExternalTask);
            }
            catch (ExternalTaskException ex)
            {
                HandleExternalTaskException(workerId, externalTask, ex);
            }
            catch (Exception ex)
            {
                HandleException(workerId, executorAttribute, externalTask, ex);
            }
        }

        public Task UnlockExternalTasks(string externalTaskId)
        {
            return _engineClient.Client().ExternalTasks[externalTaskId].Unlock();
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private IExternalTaskExecutor GetExecutor(string topicName)
        {
            return _externalTaskExecutors.First(executor => (executor.GetType().GetCustomAttributes(typeof(ExternalTaskTopicAttribute), true)
                                        .FirstOrDefault() as ExternalTaskTopicAttribute)?.TopicName == topicName);
        }
        
        private static ExternalTaskTopicAttribute GetExecutorAttributeData(IExternalTaskExecutor executor)
        {
            return executor.GetType()
                            .GetCustomAttributes(typeof(ExternalTaskTopicAttribute), true)
                            .FirstOrDefault() as ExternalTaskTopicAttribute;
        }
        
        private static async Task<Dictionary<string, object>> ExecuteExternalTask(IExternalTaskExecutor executor, ExternalTask externalTask)
        {
            var result = await executor.Execute(externalTask);

            return result;
        }
        
        private void HandleException(string workerId, ExternalTaskTopicAttribute workerAttribute, ExternalTask externalTask, Exception ex)
        {

            var retriesLeft = workerAttribute.Retries; // start with default
            if (externalTask.Retries.HasValue) // or decrement if retries are already set
            {
                retriesLeft = externalTask.Retries.Value - 1;
            }

            var externalTaskFailure = new ExternalTaskFailure
            {
                WorkerId = workerId,
                ErrorMessage = ex.Message,
                ErrorDetails = ex.StackTrace,
                RetryTimeout = workerAttribute.RetryTimeout,
                Retries = retriesLeft
            };

            _engineClient.Client().ExternalTasks[externalTask.Id].HandleFailure(externalTaskFailure);

        }
        private void HandleExternalTaskException(string workerId, ExternalTask externalTask, ExternalTaskException ex)
        {
            var externalTaskBpmnError = new ExternalTaskBpmnError
            {
                WorkerId = workerId,
                ErrorCode = ex.BusinessErrorCode
            };

            _engineClient.Client().ExternalTasks[externalTask.Id].HandleBpmnError(externalTaskBpmnError);
        }

        #endregion
    }
}
