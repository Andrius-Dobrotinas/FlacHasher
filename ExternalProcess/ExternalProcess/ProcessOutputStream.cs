using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.ExternalProcess
{
    public class ProcessOutputStream : Stream
    {
        private readonly Stream outputStream;
        private readonly TaskCompletionSource<object> outputReadTaskCompletion;
        private readonly Task processTask;
        private volatile bool isClosed;

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
                return 0;

            var readCount = ReadFromProcess(buffer, offset, count);
            if (readCount == 0)
            {
                EndOfTheLine = true;

                // Losing this race means Close got in first and cancelled the run, which processTask is about to report
                outputReadTaskCompletion.TrySetResult(null);

                // intercept cancellation/timeout exception OR
                // wait for the process to exit and throw an exception if there is one
                processTask.GetAwaiter().GetResult();
            }
            return readCount;
        }

        /// <summary>
        /// Releasing the process' end of the pipe while a read is waiting on it interrupts that read, and how it
        /// surfaces depends on the platform: Unix raises an IOException on the interrupted system call, Windows
        /// reports the stream as disposed of. Neither is a fault worth passing on - this end was closed because the
        /// caller asked for it - so both come back as an end of stream, and the cancellation is reported from there.
        /// </summary>
        private int ReadFromProcess(byte[] buffer, int offset, int count)
        {
            try
            {
                return outputStream.Read(buffer, offset, count);
            }
            catch (Exception e) when (isClosed && (e is IOException || e is ObjectDisposedException))
            {
                return 0;
            }
        }

        public override void Close()
        {
            isClosed = true;

            bool finishedPriorToThis = !outputReadTaskCompletion.TrySetCanceled();
            if (!finishedPriorToThis)
            {
                try
                {
                    // Wait for the process to exit
                    processTask.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    // Cancellation on closing has to be quiet
                }
            }

            // Nothing else lets go of the process' end of the pipe: disposing of the process leaves its streams alone
            outputStream.Dispose();

            base.Close();
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
        }
    }
}
