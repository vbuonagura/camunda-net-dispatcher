using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Camunda.Dispatcher.Contracts;
using Camunda.Dispatcher.Core;
using Camunda.Dispatcher.Entities;
using Camunda.Dispatcher.Helpers;
using SampleWebApi.Models;

namespace SampleWebApi.Executors
{

    [ExternalTaskTopic("register-attribute", "reinsurance-risk-declaration")]
    public class RegisterAttribute : IExternalTaskExecutor
    {
 
        public RegisterAttribute()
        {
        }

        public async Task<Dictionary<string, object>> Execute(ExternalTask externalTask)
        {
            var reinsuranceRiskProperties = externalTask.Variables.MapToDto<ProcessVariableDto>();

            // Your external task implementation will be here
            await Task.Run(new Action(() => Console.WriteLine("")));

            return reinsuranceRiskProperties.AsObjectValueDictionary();
        }
        
    }
}
