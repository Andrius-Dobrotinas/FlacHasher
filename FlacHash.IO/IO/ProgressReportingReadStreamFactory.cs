using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO
{
    public class ProgressReportingReadStreamFactory : IReadStreamFactory
    {
        private readonly IProgress<int> progressReporter;

        public ProgressReportingReadStreamFactory(IProgress<int> progressReporter)
        {
            this.progressReporter = progressReporter;
        }

        public Stream CreateStream(FileInfo sourceFile)
        {
            if (sourceFile.Exists == false)
                throw new InputFileNotFoundException(sourceFile.FullName);

            var stream = new ProgressReportingReadOnlyFileStream(sourceFile.FullName);

            stream.BytesRead += new BytesReadHandler(progressReporter.Report);

            return stream;
        }
    }
}