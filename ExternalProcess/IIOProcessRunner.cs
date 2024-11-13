using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.ExternalProcess
{
    public interface IIOProcessRunner
    {
        /// <summary>
        /// Runs the <paramref name="executableFile"/>, feeding it <paramref name="input"/>, and returns the output as a real-time stream.
        /// Disposes of the provided <paramref name="input"/> when it completes (regardless of whether it was successful)
        /// </summary>
        ProcessOutputStream RunAndReadOutput(
            FileInfo executableFile,
            IEnumerable<string> arguments,
            Stream input,
            CancellationToken cancellation = default);
    }
}