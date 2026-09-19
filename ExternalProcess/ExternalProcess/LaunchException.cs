using System;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that the process could not be started at all - nothing ran, so there's no process output to speak of.
    /// </summary>
    public class LaunchException : ExecutionException
    {
        public LaunchException(Exception innerException)
            : base(innerException.Message, processErrorOutput: null, isProcessOutputCaptured: false, innerException)
        {
        }
    }
}
