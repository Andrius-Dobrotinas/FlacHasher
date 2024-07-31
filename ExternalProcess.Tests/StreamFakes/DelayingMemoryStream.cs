using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.ExternalProcess
{
    class DelayingMemoryStream : MemoryStream
    {
        readonly int delayMillis;
        readonly int? maxReadSize;
        readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        public DelayingMemoryStream(byte[] source, int delayMillis, int? maxReadSize = null)
            : base(source)
        {
            this.delayMillis = delayMillis;
            this.maxReadSize = maxReadSize;
        }

        int GetChunkSize(int requestedCount)
        {
            return maxReadSize.HasValue
                ? (requestedCount > maxReadSize.Value)
                    ? maxReadSize.Value
                    : requestedCount
                : requestedCount;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                Task.Delay(delayMillis, cancellation.Token).Wait();
            }
            catch
            {
            }
            var read = base.Read(buffer, offset, GetChunkSize(count));
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                Task.Delay(delayMillis, cancellation.Token).Wait();
            }
            catch
            {
            }
            base.Write(buffer, offset, GetChunkSize(count));
        }

        public override void Close()
        {
            base.Close();
            try
            {
                cancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // just in case Close/Dispose is called more than once
            }
            cancellation.Dispose();
        }
    }
}