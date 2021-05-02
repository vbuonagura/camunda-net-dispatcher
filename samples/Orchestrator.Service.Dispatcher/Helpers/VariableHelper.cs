using System.Collections.Generic;
using Camunda.Api.Client;

namespace SampleWebApi.Helpers
{
    public static class VariableHelper
    {
        public static Dictionary<string, object> MapToMessage(this Dictionary<string, object> source)
        {
            var messageVariables = new Dictionary<string, object>();
            foreach (var item in source)
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    continue;
                }

                if (messageVariables.ContainsKey(item.Key))
                {
                    continue;
                }

                messageVariables.Add(item.Key, (item.Value as VariableValue).Value);
            }

            return messageVariables;
        }
    }
}
