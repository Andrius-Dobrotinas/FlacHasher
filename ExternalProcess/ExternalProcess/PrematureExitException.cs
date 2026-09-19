using System;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that a process stopped reading its input before all of it had been sent, and then exited successfully.
    /// It reported no fault of its own, but it was never given everything it was meant to work on.
    /// </summary>
    public class PrematureExitException : ExecutionWithExitCodeException
    {
        public PrematureExitException(int exitCode, string processErrorOutput, bool isProcessOutputCaptured)
            : base($"The process stopped reading the input before all of it had been written and exited with code {exitCode}. The result is not complete and may be corrupt.", exitCode, processErrorOutput, isProcessOutputCaptured)
        {
        }
    }
}
