namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that an external process reported its own exit code, unlike failures where none can be trusted
    /// (e.g. a process getting terminated by the runner instead of exiting on its own).
    /// </summary>
    public class ExecutionWithExitCodeException : ExecutionException
    {
        public int ExitCode { get; }

        public ExecutionWithExitCodeException(int exitCode)
            : this(exitCode, processErrorOutput: null, isProcessOutputCaptured: false)
        {
        }

        public ExecutionWithExitCodeException(int exitCode, string processErrorOutput, bool isProcessOutputCaptured)
            : this($"The process exited with code {exitCode}", exitCode, processErrorOutput, isProcessOutputCaptured)
        {
        }

        protected ExecutionWithExitCodeException(string message, int exitCode, string processErrorOutput, bool isProcessOutputCaptured)
            : base(message, processErrorOutput, isProcessOutputCaptured)
        {
            ExitCode = exitCode;
        }
    }
}
