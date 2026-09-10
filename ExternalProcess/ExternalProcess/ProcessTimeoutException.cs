using System;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that a process took longer than it was allowed and was terminated part way through its output.
    /// Whatever the consumer has read is an unfinished fragment, and there is no more coming.
    /// </summary>
    public class ProcessTimeoutException : ExecutionException
    {
        // Some callers show nothing but the message, so it has to explain itself without any help
        public ProcessTimeoutException(string processErrorOutput, bool isProcessOutputCaptured)
            : base("The process took longer than it is allowed and was terminated before it finished; its output may be incomplete", NoExitCode, processErrorOutput, isProcessOutputCaptured)
        {
        }

        /// <summary>
        /// A terminated process reports the code the OS killed it with, never the one it would have chosen.
        /// </summary>
        public const int NoExitCode = 0;
    }
}
