using System;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that a process served all of its output but then would not exit, and had to be terminated.
    /// The output is all there; what's missing is the process' own word that it did its job.
    /// </summary>
    public class ProcessNotRespondingException : ExecutionException
    {
        // Some callers show nothing but the message, so it has to explain itself without any help
        public ProcessNotRespondingException(string processErrorOutput, bool isProcessOutputCaptured)
            : base("The process stopped responding after writing all of its output (closing the std-out pipe). It had to be terminated. There is no way of knowing whether it finished the job.", NoExitCode, processErrorOutput, isProcessOutputCaptured)
        {
        }

        /// <summary>
        /// A terminated process reports the code the OS killed it with, never the one it would have chosen,
        /// so there is no exit code of the program's own to report here.
        /// </summary>
        public const int NoExitCode = 0;
    }
}
