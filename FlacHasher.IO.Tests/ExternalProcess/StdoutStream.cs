using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.IO.ExternalProcess
{
    /// <summary>
    /// Emulates the behavior of process.stdout where, once the stream is closed,
    /// Reads return EOF without throwing any exceptions
    /// The source stream has to throw <see cref="ObjectDisposedException"/> on reads after getting closed/disposed of
    /// </summary>
    class StdoutStream : Stream
    {
        readonly Stream source;

        public StdoutStream(Stream source)
        {
            this.source = source;
        }

        public override bool CanRead => source.CanRead;
        public override bool CanSeek => source.CanSeek;
        public override bool CanWrite => source.CanWrite;
        public override long Length => source.Length;
        public override long Position { get => source.Position; set => source.Position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                var readCount = source.Read(buffer, offset, count);
                return readCount;
            }
            catch (ObjectDisposedException)
            {
                return 0;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return source.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            source.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            source.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            source.Flush();
        }

        public override void Close()
        {
            source.Close();
        }
    }
}
