using System.Collections.Generic;

namespace Camunda.Dispatcher.Entities
{
    public class ExternalTask
    {
		public string Id { get; set; }
		public string WorkerId { get; set; }
        public string TopicName { get; set; }
        public string ActivityId { get; set; }
		public string ActivityInstanceId { get; set; }
		public string ProcessInstanceId { get; set; }
		public string ProcessDefinitionId { get; set; }
		public int? Retries { get; set; }
		public long? Priority { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public override string ToString() => $"ExternalTask [Id={Id}, ActivityId={ActivityId}]";
    }
}
