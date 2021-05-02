using System;
using System.Collections.Generic;

namespace Camunda.Dispatcher.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class ExternalTaskVariableRequirementsAttribute : Attribute
	{
		public List<string> VariablesToFetch { get; }

		public ExternalTaskVariableRequirementsAttribute()
		{
			VariablesToFetch = new List<string>();
		}

		public ExternalTaskVariableRequirementsAttribute(params string[] variablesToFetch)
		{
			VariablesToFetch = new List<string>(variablesToFetch);
		}

	}
}
