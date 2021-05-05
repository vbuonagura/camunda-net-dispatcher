using System;

namespace Camunda.Dispatcher.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ExternalTaskTopicAttribute : Attribute
    {
        public string TopicName { get; }
        public string ProcessId { get; }
        public int Retries { get; } = 3;
        public long RetryTimeout { get; } = 30 * 1000;

        public ExternalTaskTopicAttribute(string topicName, string processId)
        {
            TopicName = topicName;
            ProcessId = processId;
        }

        public ExternalTaskTopicAttribute(string topicName, string processId, int retries, long retryTimeout)
        {
            TopicName = topicName;
            ProcessId = processId;
            Retries = retries;
            RetryTimeout = retryTimeout;
        }
    }
}
