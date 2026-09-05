using System;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that a process took longer than it was allowed and was terminated part way through its output.
    /// Whatever the consumer has read is an unfinished fragment, and there is no more coming.
    /// </summary>
    public class ProcessTimeoutException : ExecutionException
    {
        public ProcessTimeoutException(string processErrorOutput, bool isProcessOutputCaptured)
            : base(BuildMessage(processErrorOutput, isProcessOutputCaptured), NoExitCode, processErrorOutput, isProcessOutputCaptured)
        {
        }

        /// <summary>
        /// A terminated process reports the code the OS killed it with, never the one it would have chosen.
        /// </summary>
        public const int NoExitCode = 0;

        static string BuildMessage(string processErrorOutput, bool isProcessOutputCaptured)
        {
            // Some callers show nothing but the message, so it has to explain itself without any help
            const string what = "The process took longer than it is allowed and was terminated before it finished, so its output is incomplete.";

            return isProcessOutputCaptured
                ? $"{what} Process error output\n: {processErrorOutput}"
                : $"{what} Process error output has not been captured";
        }
    }
}
