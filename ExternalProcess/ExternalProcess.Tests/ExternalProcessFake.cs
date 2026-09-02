using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.ExternalProcess
{
    class ExternalProcessFake : IExternalProcess
    {
        TaskCompletionSource<bool> voluntaryExitCompletion;
        bool respondToExitRequest;
        bool closeStreamsOnExit;
        readonly int exitCode;

        /// <summary>
        /// Wraps <paramref name="stdout"/> and <paramref name="stderr"/> in <see cref="StdoutStream"/>
        /// to provide Process' stdout and stderr behavior when the process closes:
        /// streams don't get disposed of, they just keep returning EOF on each read.
        /// </summary>
        public ExternalProcessFake(Stream stdout = null, Stream stdin = null, Stream stderr = null,
            TaskCompletionSource<bool> voluntaryExitCompletion = null,
            bool respondToExitRequest = true,
            bool closeStreamsOnExit = true,
            int exitCode = 0)
        {
            if (stdin != null)
                StandardInput = new StreamWriter(stdin);
            if (stdout != null)
                StandardOutput = new StreamReader(new StdoutStream(stdout));
            if (stderr != null)
                StandardError = new StreamReader(new StdoutStream(stderr));
            this.voluntaryExitCompletion = voluntaryExitCompletion;
            this.respondToExitRequest = respondToExitRequest;
            this.closeStreamsOnExit = closeStreamsOnExit;
            this.exitCode = exitCode;
        }

        public virtual bool HasExited { get; private set; }

        // Mirrors Process.ExitCode: unavailable until the process has actually exited (via WaitForExit or Kill)
        public virtual int ExitCode
        {
            get
            {
                if (!HasExited)
                    throw new InvalidOperationException("Process has not exited, so the exit code is not available");
                return exitCode;
            }
        }

        public virtual StreamReader StandardOutput { get; set; }
        public virtual StreamWriter StandardInput { get; set; }
        public virtual StreamReader StandardError { get; set; }

        private bool disposedOf;
        public bool IsDisposedOf
        {
            get
            {
                lock (this) return disposedOf;
            }
            private set
            {
                lock (this)
                    disposedOf = value;
            }
        }

        public virtual void Dispose()
        {
            IsDisposedOf = true;

            // The process doesn't kill the streams when it's disposed of
        }

        public virtual void Kill(bool entireProcessTree)
        {
            HasExited = true;
            if (closeStreamsOnExit)
                CloseStreams();
            voluntaryExitCompletion?.SetResult(false);
        }

        void CloseStreams()
        {
            StandardInput?.BaseStream?.Close();
            StandardOutput?.BaseStream?.Close();
            StandardError?.BaseStream?.Close();
        }

        public virtual bool Start()
        {
            return true;
        }

        public virtual bool WaitForExit(int timeoutMs)
        {
            if (respondToExitRequest)
            {
                HasExited = true;
                if (closeStreamsOnExit)
                    CloseStreams();
                voluntaryExitCompletion?.SetResult(true);
            }

            return respondToExitRequest;
        }

        // I don't feel like implementing the call to this everywhere
        /// <summary>
        /// This method is only for tests to dispose of all of this.
        /// The original Dispose-of method doesn't do anything so that tests can keep working with all of this stuff for as long as they need
        /// </summary>
        public virtual void Destroy()
        {
            StandardInput?.Dispose();
            StandardOutput?.Dispose();
            StandardError?.Dispose();
        }
    }
}
