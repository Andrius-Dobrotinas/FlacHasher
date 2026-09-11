using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// A live view of the process' standard output: handed back before the process has written anything,
    /// readable as the process returns data.
    /// When the process finishes writing, subsequent reads return <c>0</c>-bytes.
    /// The process' exit code is checked only after reading the final chunk, which results in an exception if
    /// the process exits with an error code or if other problems occur.
    /// </summary>
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

        /// <summary>
        /// Tells whether the stream has been fully read.
        /// Every read from there on returns <c>0</c> without asking the process anything again.
        /// </summary>
        public bool EndOfTheLine { get; private set; }
        public override bool CanRead => outputStream.CanRead;
        public override bool CanSeek => outputStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => outputStream.Length;
        public override long Position { get => outputStream.Position; set => outputStream.Position = value; }
        public override bool CanTimeout => outputStream.CanTimeout;
        public override int ReadTimeout { get => outputStream.ReadTimeout; set => outputStream.ReadTimeout = value; }

        /// <summary>
        /// When the output ends, returns <c>0</c> bytes, unless the process exited with an error code - in which case it throws an exception.
        /// </summary>
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
        /// Releasing the process' end of the pipe while a read is waiting on it interrupts that read.
        /// The way this surfaces depends on the platform: Unix raises an IOException on the interrupted system call,
        /// Windows reports the stream as disposed of.
        /// Neither is a fault worth passing on - this end was closed because the caller asked for it - 
        /// so both come back as an end of stream, and the cancellation is reported from there.
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

        /// <summary>
        /// Disposes of the stream without waiting for the process to finish.
        /// If the run is still in process, this cancels it.
        /// Waits for the process to actually exit before returning.
        /// Safe to call more than once.
        /// </summary>
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
