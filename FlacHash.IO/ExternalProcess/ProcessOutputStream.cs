using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.FlacHash.ExternalProcess
{
    public class ProcessOutputStream : Stream
    {
        private readonly Stream outputStream;
        private readonly TaskCompletionSource<object> outputReadTaskCompletion;
        private readonly Task processTask;

        public ProcessOutputStream(Stream outputStream, TaskCompletionSource<object> outputReadTaskCompletion, Task process)
        {
            this.outputStream = outputStream;
            this.outputReadTaskCompletion = outputReadTaskCompletion;
            processTask = process;
        }

        public bool EndOfTheLine { get; private set; }
        public override bool CanRead => outputStream.CanRead;
        public override bool CanSeek => outputStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => outputStream.Length;
        public override long Position { get => outputStream.Position; set => outputStream.Position = value; }
        public override bool CanTimeout => outputStream.CanTimeout;
        public override int ReadTimeout { get => outputStream.ReadTimeout; set => outputStream.ReadTimeout = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (EndOfTheLine)
                throw new InvalidOperationException("The stream has ended and all data has already been returned");

            var readCount = outputStream.Read(buffer, offset, count);
            if (readCount == 0)
            {
                EndOfTheLine = true;
                outputReadTaskCompletion.SetResult(null);

                // intercept cancellation/timeout exception OR
                // wait for the process to exit and throw an exception if there is one
                processTask.GetAwaiter().GetResult();
            }
            return readCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return outputStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }
    }
}
