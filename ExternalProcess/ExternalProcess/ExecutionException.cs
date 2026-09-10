using System;
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
        public bool IsProcessOutputCaptured { get; }

        public ExecutionException(int exitCode)
            : this(exitCode, processErrorOutput: null, isProcessOutputCaptured: false)
        {
        }

        public ExecutionException(int exitCode, string processErrorOutput, bool isProcessOutputCaptured)
            : base(BuildMessage(exitCode, processErrorOutput, isProcessOutputCaptured))
        {
            ExitCode = exitCode;
            ProcessErrorOutput = processErrorOutput;
            IsProcessOutputCaptured = isProcessOutputCaptured;
        }

        static string BuildMessage(int exitCode, string processErrorOutput, bool isProcessOutputCaptured)
        {
            string what = $"The process exited with code {exitCode}";

            return isProcessOutputCaptured
                ? $"{what}. Process error output\n: {processErrorOutput}"
                : what;
        }

        /// <summary>
        /// For failures that aren't an exit code: a terminated process never reports one of its own choosing,
        /// so "exited with code N" would be a lie.
        /// </summary>
        protected ExecutionException(string message, int exitCode, string processErrorOutput, bool isProcessOutputCaptured)
            : base(message)
        {
            ExitCode = exitCode;
            ProcessErrorOutput = processErrorOutput;
            IsProcessOutputCaptured = isProcessOutputCaptured;
        }
    }
}