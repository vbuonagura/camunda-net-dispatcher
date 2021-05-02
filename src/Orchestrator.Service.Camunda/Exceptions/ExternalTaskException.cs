using System;

namespace Camunda.Dispatcher.Exceptions
{
    public class ExternalTaskException : Exception
	{
		public string BusinessErrorCode { get; set; }

		public ExternalTaskException(string businessErrorCode, string message) : base(message)
		{
			BusinessErrorCode = businessErrorCode;
		}

	}
}
