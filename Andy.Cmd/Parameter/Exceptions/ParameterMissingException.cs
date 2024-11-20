using System;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    /// <summary>
    /// Indicates that parameter has not been provided by the caller
    /// </summary>
    public class ParameterMissingException : ParameterException
    {
        public PropertyInfo ParameterProperty { get; private set; }

        public ParameterMissingException(PropertyInfo param) : this($"A mandatory parameter was not supplied", param)
        {
        }

        public ParameterMissingException(string message, PropertyInfo param) : base(message)
        {
            ParameterProperty = param;
        }
    }
}
