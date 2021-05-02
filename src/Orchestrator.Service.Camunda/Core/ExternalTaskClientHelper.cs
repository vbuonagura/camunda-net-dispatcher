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
        private readonly IEnumerable<IExternalTaskAdapter> _externalWorkers;

        public ExternalTaskClientHelper(IEngineClient engineClient, IEnumerable<IExternalTaskAdapter> externalWorkers)
        {
            _engineClient = engineClient;
            _externalWorkers = externalWorkers;
        }

        #region PUBLIC METHODS

        public async Task ProcessLockedTasks(string workerId, LockedExternalTask lockedExternalTask)
        {
            var worker = GetWorker(lockedExternalTask.TopicName);
            var workerAttribute = GetWorkerAttributeData(worker);
            var externalTask = lockedExternalTask.ToExternalTask();

            try
            {
                var resultVariables = await ExecuteExternalTask(lockedExternalTask, worker, externalTask);

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
                HandleException(workerId, workerAttribute, externalTask, ex);
            }
        }

        public Task UnlockExternalTasks(string externalTaskId)
        {
            return _engineClient.Client().ExternalTasks[externalTaskId].Unlock();
        }

        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private IExternalTaskAdapter GetWorker(string topicName)
        {
            return _externalWorkers.First(externalWorker => (externalWorker.GetType().GetCustomAttributes(typeof(ExternalTaskTopicAttribute), true).FirstOrDefault()
                                        as ExternalTaskTopicAttribute)?.TopicName == topicName);
        }
        
        private ExternalTaskTopicAttribute GetWorkerAttributeData(IExternalTaskAdapter worker)
        {
            return worker.GetType().GetCustomAttributes(typeof(ExternalTaskTopicAttribute), true).FirstOrDefault()
                                        as ExternalTaskTopicAttribute;
        }
        
        private async Task<Dictionary<string, object>> ExecuteExternalTask(LockedExternalTask lockedExternalTask, IExternalTaskAdapter worker, ExternalTask externalTask)
        {
            var result = await worker.Execute(externalTask);

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
