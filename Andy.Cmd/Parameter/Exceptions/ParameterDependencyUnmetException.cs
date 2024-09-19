using System;

namespace Andy.Cmd.Parameter
{
    public class ParameterDependencyUnmetException : ParameterMissingException
    {
        public ParameterDependencyUnmetException(string paramName, string masterParamName) 
            : base($"A mandatory parameter required by {masterParamName} was not supplied", paramName)
        {
        }
    }
}
