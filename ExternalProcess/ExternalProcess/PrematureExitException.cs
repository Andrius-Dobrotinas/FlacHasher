using System;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Indicates that a process stopped reading its input before all of it had been sent, and then exited successfully.
    /// It reported no fault of its own, but it was never given everything it was meant to work on.
    /// </summary>
    public class PrematureExitException : ExecutionException
    {
        public PrematureExitException(int exitCode, string processErrorOutput, bool isProcessOutputCaptured)
            : base(BuildMessage(exitCode, processErrorOutput, isProcessOutputCaptured), exitCode, processErrorOutput, isProcessOutputCaptured)
        {
        }

        static string BuildMessage(int exitCode, string processErrorOutput, bool isProcessOutputCaptured)
        {
            // Some callers show nothing but the message, so it has to explain itself without any help
            string what = $"The process stopped reading its input before all of it had been sent, and then exited with code {exitCode}. Only part of the data reached it, so whatever it produced covers only that part.";

            return isProcessOutputCaptured
                ? $"{what} Process error output\n: {processErrorOutput}"
                : $"{what} Process error output has not been captured";
        }
    }

    /// <summary>
    /// Whether the whole of the caller's input made it to the process. Written by the task doing the feeding
    /// and read by the one reporting on the outcome, so it is deliberately the smallest thing that can carry the answer.
    /// </summary>
    class InputDelivery
    {
        volatile bool finished;

        public bool Finished => finished;

        public void MarkFinished()
        {
            finished = true;
        }
    }
}
