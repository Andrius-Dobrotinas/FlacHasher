using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.ExternalProcess
{
    public interface IOutputOnlyProcessRunner
    {
        /// <summary>
        /// Runs the <paramref name="executableFile"/> and and returns the output as a real-time stream
        /// </summary>
        ProcessOutputStream RunAndReadOutput(
            FileInfo executableFile,
            IEnumerable<string> arguments,
            CancellationToken cancellation = default);
    }
}