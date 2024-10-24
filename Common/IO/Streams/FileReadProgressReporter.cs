using System;
using System.Collections.Generic;

namespace Andy.IO
{
    public class FileReadProgressReporter : IDataReadEventSource, IProgress<int>
    {
        public event BytesReadHandler BytesRead;

        public void Report(int bytesRead)
        {
            BytesRead?.Invoke(bytesRead);
        }
    }
}