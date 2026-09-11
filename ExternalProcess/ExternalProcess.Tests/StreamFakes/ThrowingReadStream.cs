using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Throws on a specified read - to emulate a stream that fails unexpectedly while being read.
    /// <paramref name="readsBeforeThrowing"/> and <paramref name="maxReadSize"/> specify which read to throw on
    /// so that some of the source has come through by the time it happens.
    /// </summary>
    class ThrowingReadStream : MemoryStream
    {
        readonly int readsBeforeThrowing;
        readonly int maxReadSize;
        int reads;

        public ThrowingReadStream(byte[] source, int readsBeforeThrowing = 0, int maxReadSize = int.MaxValue) : base(source)
        {
            this.readsBeforeThrowing = readsBeforeThrowing;
            this.maxReadSize = maxReadSize;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (reads++ >= readsBeforeThrowing)
                throw new IOException("Emulated read failure");

            return base.Read(buffer, offset, Math.Min(count, maxReadSize));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
