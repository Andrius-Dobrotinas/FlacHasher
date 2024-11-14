using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Signals on reads and closing
    /// </summary>
    class ReadSignallingMemoryStream : MemoryStream
    {
        ManualResetEventSlim readFinishSignal;
        ManualResetEventSlim closeSignal;
        AutoResetEvent readSignal;

        public ReadSignallingMemoryStream(byte[] data, AutoResetEvent readSignal = null, ManualResetEventSlim readFinishSignal = null, ManualResetEventSlim closeSignal = null) : base(data)
        {
            this.readSignal = readSignal;
            this.readFinishSignal = readFinishSignal;
            this.closeSignal = closeSignal;
        }

        public override void Close()
        {
            base.Close();
            closeSignal?.Set();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readCount = base.Read(buffer, offset, count);
            readSignal?.Set();
            if (readCount == 0)
            {
                readFinishSignal?.Set();
            }
            return readCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}