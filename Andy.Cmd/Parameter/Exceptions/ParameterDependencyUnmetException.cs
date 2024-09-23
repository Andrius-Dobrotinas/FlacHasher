using System;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public class ParameterDependencyUnmetException : ParameterMissingException
    {
        public ParameterDependencyUnmetException(PropertyInfo param, string masterParamName)
            : base($"A mandatory parameter required by {masterParamName} was not supplied", param)
        {
        }
    }
}
