using System;

namespace Andy.Cmd.Parameter
{
    public class ParameterException : Exception
    {
        public string ParameterName { get; set; }

        public ParameterException(string message, string paramName) : base($"{message}: {paramName}")
        {
            ParameterName = paramName;
        }
    }
}
