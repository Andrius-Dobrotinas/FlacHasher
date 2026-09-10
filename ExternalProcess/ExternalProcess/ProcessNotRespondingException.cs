using System;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that a process served all of its output but then would not exit, and had to be terminated.
    /// The output is all there; what's missing is the process' own word that it did its job.
    /// </summary>
    public class ProcessNotRespondingException : ExecutionException
    {
        public ProcessNotRespondingException(string processErrorOutput, bool isProcessOutputCaptured)
            : base(BuildMessage(processErrorOutput, isProcessOutputCaptured), NoExitCode, processErrorOutput, isProcessOutputCaptured)
        {
        }

        /// <summary>
        /// A terminated process reports the code the OS killed it with, never the one it would have chosen,
        /// so there is no exit code of the program's own to report here.
        /// </summary>
        public const int NoExitCode = 0;

        static string BuildMessage(string processErrorOutput, bool isProcessOutputCaptured)
        {
            // Some callers show nothing but the message, so it has to explain itself without any help
            const string what = "The process stopped responding after writing all of its output (closing the std-out pipe). It had to be terminated. There is no way of knowing whether it finished the job.";

            return isProcessOutputCaptured
                ? $"{what}. Process error output\n: {processErrorOutput}"
                : what;
        }
    }
}
