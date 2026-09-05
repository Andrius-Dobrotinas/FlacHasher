using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.ExternalProcess
{
    class ExternalProcessFake : IExternalProcess
    {
        /// <summary>
        /// What an OS reports for a killed process: Windows terminates it with -1, Unix' SIGKILL surfaces as 128 + 9.
        /// A killed process never gets to report the exit code it would have exited with on its own.
        /// </summary>
        public static int KillExitCode => OperatingSystem.IsWindows() ? -1 : 137;

        readonly TaskCompletionSource<int> exitCompletion = new TaskCompletionSource<int>();
        readonly TaskCompletionSource<object> killRequestCompletion = new TaskCompletionSource<object>();
        readonly TaskCompletionSource<bool> voluntaryExitCompletion;
        readonly bool respondToExitRequest;
        readonly bool closeStreamsOnExit;
        readonly bool exitOnKillRequest;
        readonly int exitCode;
        bool started;

        /// <summary>
        /// Wraps <paramref name="stdout"/> and <paramref name="stderr"/> in <see cref="StdoutStream"/>
        /// to provide Process' stdout and stderr behavior when the process closes:
        /// streams don't get disposed of, they just keep returning EOF on each read.
        /// </summary>
        /// <param name="respondToExitRequest">When on, the process exits on its own as soon as someone waits for it to exit</param>
        /// <param name="exitOnKillRequest">When off, the process stays alive after <see cref="Kill"/> until a test calls <see cref="CompleteKill"/>.
        /// Use it to test what happens in the window between termination being requested and the process actually dying.</param>
        public ExternalProcessFake(Stream stdout = null, Stream stdin = null, Stream stderr = null,
            TaskCompletionSource<bool> voluntaryExitCompletion = null,
            bool respondToExitRequest = true,
            bool closeStreamsOnExit = true,
            bool exitOnKillRequest = true,
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
            this.exitOnKillRequest = exitOnKillRequest;
            this.exitCode = exitCode;
        }

        public virtual bool HasExited => exitCompletion.Task.IsCompleted;

        // Mirrors Process.ExitCode: unavailable until the process has actually exited
        public virtual int ExitCode
        {
            get
            {
                if (!HasExited)
                    throw new InvalidOperationException("Process has not exited, so the exit code is not available");
                return exitCompletion.Task.Result;
            }
        }

        public virtual StreamReader StandardOutput { get; set; }
        public virtual StreamWriter StandardInput { get; set; }
        public virtual StreamReader StandardError { get; set; }

        public bool IsKillRequested => killRequestCompletion.Task.IsCompleted;

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

        /// <summary>
        /// Mirrors Process.Kill on .NET (Core): it merely requests termination and returns before the process is actually gone,
        /// and killing an already-exited process is a quiet no-op.
        /// </summary>
        public virtual void Kill(bool entireProcessTree)
        {
            if (!started) throw new InvalidOperationException("There is no process associated with this object");

            if (HasExited) return;

            killRequestCompletion.TrySetResult(null);

            if (exitOnKillRequest)
                Task.Run(CompleteKill);
        }

        /// <summary>
        /// Makes a process that's been asked to terminate actually die - which the real thing does on its own time, after Kill has returned
        /// </summary>
        public void CompleteKill()
        {
            if (!IsKillRequested)
                throw new InvalidOperationException("The process has not been asked to terminate");

            if (Exit(KillExitCode))
                voluntaryExitCompletion?.TrySetResult(false);
        }

        /// <summary>
        /// Lets a test keep the process alive up until the moment it's been asked to terminate
        /// </summary>
        public bool WaitForKillRequest(int timeoutMs)
        {
            return killRequestCompletion.Task.Wait(timeoutMs);
        }

        bool Exit(int code)
        {
            var isFirstExit = exitCompletion.TrySetResult(code);
            if (isFirstExit && closeStreamsOnExit)
                CloseStreams();

            return isFirstExit;
        }

        void CloseStreams()
        {
            StandardInput?.BaseStream?.Close();
            StandardOutput?.BaseStream?.Close();
            StandardError?.BaseStream?.Close();
        }

        public virtual bool Start()
        {
            started = true;
            return true;
        }

        public virtual bool WaitForExit(int timeoutMs)
        {
            if (!started) throw new InvalidOperationException("There is no process associated with this object");

            if (respondToExitRequest && Exit(exitCode))
                voluntaryExitCompletion?.TrySetResult(true);

            return exitCompletion.Task.Wait(timeoutMs);
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
