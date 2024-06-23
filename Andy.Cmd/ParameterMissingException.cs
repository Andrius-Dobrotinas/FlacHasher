using System;

namespace Andy.Cmd
{
    /// <summary>
    /// Indicates that a command line parameter has not been provided by the caller
    /// </summary>
    public class ParameterMissingException : ParameterException
    {
        public ParameterMissingException(string paramName) : base("Parameter is missing", paramName)
        {
        }
    }
}
