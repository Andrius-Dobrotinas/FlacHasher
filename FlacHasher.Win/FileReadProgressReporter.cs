using Andy.FlacHash.IO;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win
{
    public class FileReadProgressReporter : IFileReadEventSource, IProgress<int>
    {
        public event BytesReadHandler BytesRead;

        public void Report(int bytesRead)
        {
            BytesRead?.Invoke(bytesRead);
        }
    }
}