using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.ExternalProcess
{
    public interface IIOProcessRunner
    {
        /// <summary>
        /// Runs the <paramref name="executableFile"/>, feeding it <paramref name="input"/>, and returns output once it finishes
        /// </summary>
        Stream RunAndReadOutput(
            FileInfo executableFile,
            IEnumerable<string> arguments,
            Stream input,
            CancellationToken cancellation = default);
    }
}