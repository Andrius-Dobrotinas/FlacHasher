using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win.IO
{
    public delegate void BytesReadHandler(long fileSize, long currentPosition, int byteCount);

    public class ProgressReportingReadOnlyFileStream : FileStream
    {
        public event BytesReadHandler BytesRead;

        public ProgressReportingReadOnlyFileStream(string path)
            : base(path, FileMode.Open, FileAccess.Read)
        {

        }

        public override int ReadByte()
        {
            throw new NotSupportedException("This could be slow");
        }

        public override int Read(Span<byte> buffer)
        {
            var byteCount = base.Read(buffer);

            RaiseBytesReadEvent(byteCount);

            return byteCount;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var byteCount = base.Read(buffer, offset, count);

            RaiseBytesReadEvent(byteCount);

            return byteCount;
        }

        private void RaiseBytesReadEvent(int byteCount)
        {
            BytesRead?.Invoke(Length, Position, byteCount);
        }
    }
}