using System;

namespace Andy.Cmd.Parameter
{
    public class ParameterValueException : ParameterException
    {
        public string ParameterName { get; set; }

        public ParameterValueException(string message, string paramName) : base($"{message}: {paramName}")
        {
            ParameterName = paramName;
        }
    }
}
