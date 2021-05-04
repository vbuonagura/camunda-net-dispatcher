using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Camunda.Dispatcher.Contracts;
using Camunda.Dispatcher.Core;
using Camunda.Dispatcher.Entities;
using Camunda.Dispatcher.Helpers;
using SampleWebApi.Models;

namespace SampleWebApi.Dispatchers
{

    [ExternalTaskTopic("reinsurance-update-process-status", "reinsurance-risk-declaration")]
    public class UpdateProcessStatus : IExternalTaskExecutor
    {
 
        public UpdateProcessStatus()
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
