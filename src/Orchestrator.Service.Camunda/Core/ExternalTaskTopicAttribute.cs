using System;

namespace Camunda.Dispatcher.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ExternalTaskTopicAttribute : Attribute
    {
        public string TopicName { get; }
        public string ProcessName { get; }
        public int Retries { get; } = 3;
        public long RetryTimeout { get; } = 30 * 1000;

        public ExternalTaskTopicAttribute(string topicName, string processName)
        {
            TopicName = topicName;
            ProcessName = processName;
        }

        public ExternalTaskTopicAttribute(string topicName, string processName, int retries, long retryTimeout)
        {
            TopicName = topicName;
            ProcessName = processName;
            Retries = retries;
            RetryTimeout = retryTimeout;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ExternalTaskTopicIdentifierAttribute : Attribute
    {
        public string TopicIdentifier { get; set; }

        public ExternalTaskTopicIdentifierAttribute(string topicIdentifier)
        {
            TopicIdentifier = topicIdentifier;
        }

        public ExternalTaskTopicIdentifierAttribute()
        {

        }
    }
}
