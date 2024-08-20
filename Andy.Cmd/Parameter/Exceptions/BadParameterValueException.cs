using System;
using System.Collections.Generic;

namespace Andy.Cmd.Parameter
{
    public class BadParameterValueException : ParameterException
    {
        public Exception OriginalException { get; set; }

        public BadParameterValueException(string paramName, string message) : base($"Invalid parameter value: {message}", paramName)
        {
        }

        public BadParameterValueException(string paramName, Exception e) : this(e.Message, paramName)
        {
        }
    }
}
