using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.FlacHash.IO
{
    /// <summary>
    /// Writes data nowhere.
    /// Signals on each Write and on Close.
    /// </summary>
    class FakeWriteStream : Stream
    {
        AutoResetEvent writeSignal;
        ManualResetEventSlim closeSignal;

        public FakeWriteStream(AutoResetEvent writeSignal = null, ManualResetEventSlim closeSignal = null)
        {
            this.writeSignal = writeSignal;
            this.closeSignal = closeSignal;
        }

        public override bool CanRead => throw new NotImplementedException();
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        
        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            writeSignal?.Set();
        }

        public override void Close()
        {
            closeSignal?.Set();
        }
    }
}