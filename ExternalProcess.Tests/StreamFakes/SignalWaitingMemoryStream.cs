using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.ExternalProcess
{
    class SignalWaitingMemoryStream : MemoryStream
    {
        AutoResetEvent readSignal;
        int? maxReadSize;

        public SignalWaitingMemoryStream(byte[] source, AutoResetEvent readSignal, int? maxReadSize = null) : base(source)
        {
            this.readSignal = readSignal;
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
            readSignal.WaitOne();
            return base.Read(buffer, offset, GetChunkSize(count));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            base.Close();

            // Release read from waiting
            readSignal.Set();
        }
    }
}