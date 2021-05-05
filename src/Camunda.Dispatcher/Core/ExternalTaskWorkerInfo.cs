using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Review.Process.Camunda.Core
{
    public class ExternalTaskWorkerInfo
    {
        public int Retries { get; internal set; }
        public long RetryTimeout { get; internal set; }
        public Type Type { get; internal set; }
        public string TopicName { get; internal set; }
        public string TopicIdentifier { get; set; }
        public List<string> VariablesToFetch { get; internal set; }
    }
    
    public class WorkerIdentifier
    {
        [JsonProperty(PropertyName = "value")]
        public string Identifier { get; set; }
    }

    public class RetryCount
    {
        [JsonProperty(PropertyName = "value")]
        public int Count { get; set; }
    }

    public class TimeOut
    {
        [JsonProperty(PropertyName = "value")]
        public long RetryTimeOut { get; set; }
    }

    public class WorkerRetryConfiguration
    {
        public WorkerIdentifier WorkerIdentifier { get; set; }
        public RetryCount RetryCount { get; set; }
        public TimeOut TimeOut { get; set; }
    }
}
