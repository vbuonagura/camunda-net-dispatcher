using System;

namespace Camunda.Dispatcher.Exceptions
{
    public class ExternalApiException : Exception
    {
        public ExternalApiException(string message) : base(message)
        {
        }
    }
}
