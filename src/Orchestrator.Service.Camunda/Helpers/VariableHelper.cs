using System;
using System.Collections.Generic;
using Camunda.Api.Client;

namespace Camunda.Dispatcher.Helpers
{
    public static class VariableHelper
    {
        public static T MapToDto<T>(this Dictionary<string, object> source) where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                var prop = someObjectType.GetProperty(item.Key);
                if (prop == null)
                    continue;

                prop.SetValue(someObject, (item.Value as VariableValue)?.Value, null);
            }

            return someObject;
        }

        public static Dictionary<string, object> AsObjectValueDictionary(this object data)
        {
            var variables = new Dictionary<string, object>();

            foreach (var prop in data.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(DateTime))
                {
                    variables.Add(prop.Name, ((DateTime)prop.GetValue(data)).ToString("o"));
                }
                else if (prop.PropertyType == typeof(DateTime?))
                {
                    var dateValue = (DateTime?)prop.GetValue(data);
                    variables.Add(prop.Name, dateValue?.ToString("o"));
                }
                else
                {
                    variables.Add(prop.Name, prop.GetValue(data));
                }
            }
            return variables;
        }

        public static Dictionary<string, object> ToObjectDictionary(this IDictionary<string, VariableValue> source)
        {
            var result = new Dictionary<string, object>();
            foreach (var item in source)
            {
                result.Add(item.Key, item.Value);
            }
            return result;
        }

        public static Dictionary<string, VariableValue> ToVariableDictionary(this IDictionary<string, object> source)
        {
            var result = new Dictionary<string, VariableValue>();
            foreach (var item in source)
            {
                result.Add(item.Key, VariableValue.FromObject(item.Value));
            }
            return result;
        }
    }
}
