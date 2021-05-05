using System;

namespace SampleWebApi.ConfigurationSettings
{
    public class OrchestratorServiceDispatcherSettings
    {
        public Uri MessagingServiceApiUrl { get; set; }
        public Uri DocumentServiceApiUrl{ get; set; }
        public Uri PensionServiceApiUrl { get; set; }
    }
}
