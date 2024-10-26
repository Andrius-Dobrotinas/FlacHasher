﻿using System;
using System.Collections.Generic;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that an external process exited with an error
    /// </summary>
    public class ExecutionException : Exception
    {
        public int ExitCode { get; }
        public string ProcessErrorOutput { get; }

        public ExecutionException(int exitCode)
            : base($"The process exited with code {exitCode}. Process error output has not been captured")
        {
            ExitCode = exitCode;
        }

        public ExecutionException(int exitCode, string processErrorOutput)
            : base($"The process exited with code {exitCode}. Process error output\n: {processErrorOutput}")
        {
            ExitCode = exitCode;
            ProcessErrorOutput = processErrorOutput;
        }
    }
}