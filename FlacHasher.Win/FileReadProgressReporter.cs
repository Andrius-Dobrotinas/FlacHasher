using Andy.FlacHash.Win.IO;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win
{
    public class FileReadProgressReporter : IFileReadProgressWatcher
    {
        public event BytesReadHandler BytesRead;

        public void UpdateProgress(long fileSize, long currentPosition, int bytesRead)
        {
            BytesRead?.Invoke(fileSize, currentPosition, bytesRead);
        }
    }
}