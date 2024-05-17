using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.ExternalProcess
{
    public interface IIOProcessRunner
    {
        Stream RunAndReadOutput(
            FileInfo fileToRun,
            IEnumerable<string> arguments,
            Stream input,
            CancellationToken cancellation = default);
    }
}