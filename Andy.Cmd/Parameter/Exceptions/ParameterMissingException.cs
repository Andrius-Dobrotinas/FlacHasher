using System;

namespace Andy.Cmd.Parameter
{
    /// <summary>
    /// Indicates that a command line parameter has not been provided by the caller
    /// </summary>
    public class ParameterMissingException : ParameterException
    {
        public ParameterMissingException(string paramName) : this($"A mandatory parameter was not supplied", paramName)
        {
        }

        public ParameterMissingException(string message, string paramName) : base(message, paramName)
        {
        }
    }
}
