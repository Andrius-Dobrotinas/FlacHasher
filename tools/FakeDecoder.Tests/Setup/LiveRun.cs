using CliWrap;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.FakeDecoder
{
    /// <summary>
    /// Runs the fake program while holding on to the live standard output stream, so that a test can observe
    /// <em>when</em> things happen - when the first bytes show up, when the EOF arrives, whether the process is
    /// still running at that point - rather than only what came out in the end.
    /// </summary>
    class LiveRun : IDisposable
    {
        const int killTimeoutMs = 5000;

        readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        readonly ManualResetEventSlim outputStarted = new ManualResetEventSlim(false);
        readonly ManualResetEventSlim stderrStarted = new ManualResetEventSlim(false);
        readonly ManualResetEventSlim stdoutEof = new ManualResetEventSlim(false);
        readonly MemoryStream stdout = new MemoryStream();
        readonly StringBuilder stderr = new StringBuilder();

        LiveRun(byte[] stdin, string[] arguments)
        {
            var command = App.BuildCommand(arguments)
                .WithStandardOutputPipe(PipeTarget.Create(Consume))
                .WithStandardErrorPipe(PipeTarget.Merge(
                    PipeTarget.ToStringBuilder(stderr),
                    PipeTarget.ToDelegate(_ => stderrStarted.Set())));

            if (stdin != null)
                command = command.WithStandardInputPipe(PipeSource.FromBytes(stdin));

            ProcessTask = command
                .ExecuteAsync(cancellation.Token)
                .Task;
        }

        public static LiveRun Start(params string[] arguments)
        {
            return new LiveRun(stdin: null, arguments);
        }

        public static LiveRun Start(byte[] stdin, params string[] arguments)
        {
            return new LiveRun(stdin, arguments);
        }

        /// <summary>
        /// Completes once the process has exited AND everything it wrote has been consumed.
        /// </summary>
        public Task<CommandResult> ProcessTask { get; }

        public bool StdoutHasReachedEof => stdoutEof.IsSet;

        public string StdErr => stderr.ToString();

        public byte[] Bytes
        {
            get
            {
                lock (stdout)
                    return stdout.ToArray();
            }
        }

        public bool WaitForOutput(int timeoutMs) => outputStarted.Wait(timeoutMs);

        /// <summary>
        /// The first line of standard error is the only sign of life a run that writes nothing to stdout gives,
        /// so it's what a test has to sync on before timing anything that follows.
        /// </summary>
        public bool WaitForStdErr(int timeoutMs) => stderrStarted.Wait(timeoutMs);

        public bool WaitForStdoutEof(int timeoutMs) => stdoutEof.Wait(timeoutMs);

        async Task Consume(Stream source, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];

            while (true)
            {
                int byteCount = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (byteCount == 0)
                    break;

                lock (stdout)
                    stdout.Write(buffer, 0, byteCount);

                outputStarted.Set();
            }

            // An EOF that only turned up because the test killed the process says nothing about the program letting go of stdout
            if (!cancellation.IsCancellationRequested)
                stdoutEof.Set();
        }

        public void Dispose()
        {
            // Several of these runs are meant never to end on their own, so the process has to be killed rather than waited out
            cancellation.Cancel();

            try
            {
                ProcessTask.Wait(killTimeoutMs);
            }
            catch (AggregateException)
            {
                // Cancellation is how those runs are meant to end, so the exception it produces is the expected outcome
            }

            cancellation.Dispose();
            outputStarted.Dispose();
            stderrStarted.Dispose();
            stdoutEof.Dispose();
            stdout.Dispose();
        }
    }
}
