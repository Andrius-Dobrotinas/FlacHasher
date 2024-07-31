using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.ExternalProcess
{
    public interface IOutputOnlyProcessRunner
    {
        /// <summary>
        /// Runs the <paramref name="executableFile"/> and returns output once it finishes
        /// </summary>
        Stream RunAndReadOutput(
            FileInfo executableFile,
            IEnumerable<string> arguments,
            CancellationToken cancellation = default);
    }
}