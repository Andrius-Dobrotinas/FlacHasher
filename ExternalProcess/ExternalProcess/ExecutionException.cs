using System;
using System.Collections.Generic;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that an external process exited with an error
    /// </summary>
    public class ExecutionException : Exception
    {
        public string ProcessErrorOutput { get; }
        public bool IsProcessOutputCaptured { get; }

        protected ExecutionException(string message, string processErrorOutput, bool isProcessOutputCaptured, Exception innerException = null)
            : base(message, innerException)
        {
            ProcessErrorOutput = processErrorOutput;
            IsProcessOutputCaptured = isProcessOutputCaptured;
        }
    }
}