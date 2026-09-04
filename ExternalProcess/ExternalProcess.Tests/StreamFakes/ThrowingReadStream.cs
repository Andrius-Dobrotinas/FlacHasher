using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Throws on every read, to emulate a stream that fails unexpectedly while being read
    /// </summary>
    class ThrowingReadStream : MemoryStream
    {
        public ThrowingReadStream(byte[] source) : base(source)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new IOException("Emulated read failure");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
