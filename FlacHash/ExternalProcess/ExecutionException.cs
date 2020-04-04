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
        {
            ExitCode = exitCode;
            ProcessErrorOutput = processErrorOutput;
        }
    }
}