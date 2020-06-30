using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Andy.FlacHash.Win.IO
{
    public class ProgressReportingReadStreamFactory : IReadStreamFactory
    {
        private readonly FileReadProgressReporter progressReporter;

        public ProgressReportingReadStreamFactory(FileReadProgressReporter progressReporter)
        {
            this.progressReporter = progressReporter;
        }

        public Stream CreateStream(FileInfo sourceFile)
        {
            var stream = new ProgressReportingReadOnlyFileStream(sourceFile.FullName);

            stream.BytesRead += new BytesReadHandler(progressReporter.UpdateProgress);

            return stream;
        }
    }
}