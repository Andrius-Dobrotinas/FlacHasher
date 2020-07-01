using System;
using System.Collections.Generic;

namespace Andy.FlacHash.ExternalProcess
{
    /// <summary>
    /// Indicates that an external process exited with an error
    /// </summary>
    public class ExecutionException : Exception
    {
        public int ExitCode { get; }
        public string ProcessErrorOutput { get; }

        public ExecutionException(int exitCode, string processErrorOutput)
            : base($"The process exited with code {exitCode}. See {nameof(ProcessErrorOutput)} for details.")
        {
            ExitCode = exitCode;
            ProcessErrorOutput = processErrorOutput;
        }
    }
}