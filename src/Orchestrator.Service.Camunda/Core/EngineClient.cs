using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Camunda.Api.Client;
using Camunda.Dispatcher.Contracts;
using Camunda.Dispatcher.Entities;
using Microsoft.Extensions.Options;

namespace Camunda.Dispatcher.Core
{
    public class EngineClient : IEngineClient
    {
        private static CamundaClient _camunda;
        private readonly CamundaSettings _camundaSettings;
        private readonly ProxySettings _proxySettings;

        public EngineClient(IOptions<CamundaSettings> camundaSettings, IOptions<ProxySettings> proxySettings)
        {
            _camundaSettings = camundaSettings.Value;
            _proxySettings = proxySettings.Value;
        }

        public CamundaClient Client()
        {
            if (_camunda != null) 
                return _camunda;
            
            var proxy = new WebProxy
            {
                BypassProxyOnLocal = true
            };

            if (_proxySettings.IsEnabled)
            {
                proxy.Address = new Uri(_proxySettings.Url);
                proxy.UseDefaultCredentials = true;
            }

            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy
            };

            var httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(_camundaSettings.Url)
            };

            if (!string.IsNullOrEmpty(_camundaSettings.Username))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{_camundaSettings.Username}:{_camundaSettings.Password}");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            _camunda = CamundaClient.Create(httpClient);
            return _camunda;
        }
    }
}
