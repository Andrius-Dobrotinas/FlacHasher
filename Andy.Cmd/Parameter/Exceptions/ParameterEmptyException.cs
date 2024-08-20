using System;

namespace Andy.Cmd.Parameter
{
    public class ParameterEmptyException : ParameterException
    {
        public ParameterEmptyException(string paramName) : base("Parameter supplied without a value", paramName)
        {
        }
    }
}
