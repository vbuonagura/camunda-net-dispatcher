using Camunda.Api.Client.ExternalTask;
using Camunda.Dispatcher.Entities;
using Camunda.Dispatcher.Helpers;

namespace Camunda.Dispatcher.Core
{
    public static class ExternalTaskHelper
    {
        public static ExternalTask ToExternalTask(this LockedExternalTask lockedExternalTask)
        {
            if (lockedExternalTask == null)
                return null;

            return new ExternalTask
            {
                Id = lockedExternalTask.Id,
                WorkerId = lockedExternalTask.WorkerId,
                TopicName = lockedExternalTask.TopicName,
                ActivityId = lockedExternalTask.ActivityId,
                ActivityInstanceId = lockedExternalTask.ActivityInstanceId,
                ProcessInstanceId = lockedExternalTask.ProcessInstanceId,
                ProcessDefinitionId = lockedExternalTask.ProcessDefinitionId,
                Retries = lockedExternalTask.Retries,
                Priority = lockedExternalTask.Priority,
                Variables = lockedExternalTask.Variables.ToObjectDictionary()
            };
        }
    }
}
