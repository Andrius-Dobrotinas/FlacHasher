﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.IO
{
    public delegate void BytesReadHandler(int byteCount);

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

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var byteCount = await base.ReadAsync(buffer, offset, count, cancellationToken);
            
            RaiseBytesReadEvent(byteCount);

            return byteCount;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var byteCount = await base.ReadAsync(buffer, cancellationToken);

            RaiseBytesReadEvent(byteCount);

            return byteCount;
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            base.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return base.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        private void RaiseBytesReadEvent(int byteCount)
        {
            BytesRead?.Invoke(byteCount);
        }
    }
}