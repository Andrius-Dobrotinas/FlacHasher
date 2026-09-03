using CliWrap;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.FakeCmdline
{
    record AppResult(int ExitCode, byte[] StdOut, string StdErr);

    /// <summary>
    /// Launches the fake program the way an outsider would.
    /// Deliberately built on CliWrap rather than on the process runner these tests exist to support: the stub has to
    /// be proven by something other than the code it exists to test, or one bug turns both red and neither is
    /// distinguishable from the other.
    /// </summary>
    static class App
    {
        /// <summary>
        /// Generous enough not to trip over a slow machine, short enough that a stub that hangs
        /// kills its own test instead of the whole run.
        /// </summary>
        public const int TimeoutMs = 10000;

        public static Task<AppResult> Run(params string[] arguments)
        {
            return Run(stdin: null, arguments);
        }

        public static async Task<AppResult> Run(byte[] stdin, params string[] arguments)
        {
            using (var cancellation = new CancellationTokenSource(TimeoutMs))
            using (var stdout = new MemoryStream())
            {
                var stderr = new StringBuilder();

                var command = BuildCommand(arguments)
                    // Never ExecuteBufferedAsync: it decodes standard output as text, which mangles a binary payload
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdout))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr));

                if (stdin != null)
                    command = command.WithStandardInputPipe(PipeSource.FromBytes(stdin));

                var result = await command.ExecuteAsync(cancellation.Token);

                var stdoutBytes = stdout.ToArray();

                Log(result.ExitCode, $"{stdoutBytes.Length} bytes: {Convert.ToHexString(stdoutBytes)}", stderr.ToString());

                return new AppResult(result.ExitCode, stdoutBytes, stderr.ToString());
            }
        }

        public static Command BuildCommand(string[] arguments)
        {
            return Cli.Wrap(TestEnvironment.Executable.FullName)
                .WithArguments(arguments)
                // Non-zero exit codes are what several of these tests are about, so they must not be turned into exceptions
                .WithValidation(CommandResultValidation.None);
        }

        static void Log(int exitCode, string stdout, string stderr)
        {
            Console.WriteLine($"Exit code: {exitCode}\nStandard output: {stdout}\nStandard error: {stderr}");
        }
    }
}
