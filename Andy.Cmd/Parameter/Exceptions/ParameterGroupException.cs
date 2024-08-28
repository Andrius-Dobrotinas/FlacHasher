using System;

namespace Andy.Cmd.Parameter
{
    public class ParameterGroupException : Exception
    {
        public ParameterGroupException(string message, params string[] paramNames)
            : base($"{message}: {string.Join(',', paramNames)}")
        {
        }
    }
}
