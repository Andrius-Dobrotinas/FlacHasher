using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Andy.FlacHash.IO
{
    /// <summary>
    /// Keeps the stream pumping data, with a delay between each read, until <see cref="Close"/>d.
    /// Cancels delays on closing/disposing of.
    /// Throws <see cref="ObjectDisposedException"/> after closing/disposing of.
    /// </summary>
    class EndlessFakeReadStream : Stream
    {
        readonly AutoResetEvent operationSignal;
        readonly int maxReadSize;
        readonly int? delay;
        readonly CancellationTokenSource readCancellation = new CancellationTokenSource();

        public EndlessFakeReadStream(int maxReadSize = -1, AutoResetEvent operationSignal = null, int? delayMs = null)
        {
            this.operationSignal = operationSignal;
            this.maxReadSize = maxReadSize;
            this.delay = delayMs;
        }

        public override long Length => throw new InvalidOperationException();
        public override long Position { get => position; set => throw new InvalidOperationException(); }
        long position;
        public override bool CanWrite => false;
        public override bool CanSeek => false;
        public override bool CanRead => true;

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (delay.HasValue)
                try
                {
                    Task.Delay(delay.Value, readCancellation.Token).Wait();
                }
                catch
                {
                }

            operationSignal?.WaitOne();

            // Emulate disposing of the stream
            if (readCancellation.IsCancellationRequested)
                throw new ObjectDisposedException("Fake stream has been closed/disposed of");

            var chunkSize = maxReadSize > -1 ? maxReadSize : count;
            position += chunkSize;

            return chunkSize;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
            operationSignal?.WaitOne();
        }

        public override void Close()
        {
            base.Close();

            // Release read from waiting
            operationSignal?.Set();
            try
            {
                readCancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // just in case Close/Dispose is called more than once
            }
            readCancellation.Dispose();
        }
    }
}
