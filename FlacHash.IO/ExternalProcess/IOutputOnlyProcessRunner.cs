using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.ExternalProcess
{
    public interface IOutputOnlyProcessRunner
    {
        /// <summary>
        /// Runs a process and returns the contents of its output stream
        /// </summary>
        Stream RunAndReadOutput(
            FileInfo fileToRun,
            IEnumerable<string> arguments,
            CancellationToken cancellation = default);
    }
}