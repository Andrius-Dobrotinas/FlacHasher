using System;

namespace Andy.Cmd.Parameter
{
    public class ParameterEmptyException : ParameterValueException
    {
        public ParameterEmptyException(string paramName) : base("Parameter was supplied without a value", paramName)
        {
        }
    }
}
