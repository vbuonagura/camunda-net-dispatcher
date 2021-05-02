namespace Camunda.Dispatcher.Entities
{
    public class CamundaSettings
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int ExternalTaskLockDuration { get; set; }
        public int FetchTaskCount { get; set; }
        public int PollingIntervalInMilliseconds { get; set; }
    }
}
