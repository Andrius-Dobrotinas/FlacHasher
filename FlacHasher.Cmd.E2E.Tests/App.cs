using System.Text;
using CliWrap;
using CliWrap.Buffered;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    record AppResult(int ExitCode, string StdOut, string StdErr);

    record AppRawResult(int ExitCode, byte[] StdOut, string StdErr);

    static class App
    {
        public static async Task<AppResult> Run(DirectoryInfo workingDirectory, TimeSpan timeout, params string[] arguments)
        {
            using (var cancellation = new CancellationTokenSource(timeout))
            {
                var result = await BuildCommand(workingDirectory, arguments)
                    .ExecuteBufferedAsync(cancellation.Token);

                Log(result.ExitCode, result.StandardOutput, result.StandardError);

                return new AppResult(result.ExitCode, result.StandardOutput, result.StandardError);
            }
        }

        public static async Task<AppResult> Run(DirectoryInfo workingDirectory, params string[] arguments)
        {
            return await Run(workingDirectory, TimeSpan.FromMinutes(1), arguments);
        }

        /// <summary>
        /// Captures standard output as bytes, bypassing text decoding, which would mangle non-textual output.
        /// </summary>
        public static async Task<AppRawResult> RunRaw(DirectoryInfo workingDirectory, TimeSpan timeout, params string[] arguments)
        {
            using (var cancellation = new CancellationTokenSource(timeout))
            using (var stdout = new MemoryStream())
            {
                var stderr = new StringBuilder();

                var result = await BuildCommand(workingDirectory, arguments)
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdout))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
                    .ExecuteAsync(cancellation.Token);

                var stdoutBytes = stdout.ToArray();

                Log(result.ExitCode, $"{stdoutBytes.Length} bytes: {Convert.ToHexString(stdoutBytes)}", stderr.ToString());

                return new AppRawResult(result.ExitCode, stdoutBytes, stderr.ToString());
            }
        }

        public static async Task<AppRawResult> RunRaw(DirectoryInfo workingDirectory, params string[] arguments)
        {
            return await RunRaw(workingDirectory, TimeSpan.FromMinutes(1), arguments);
        }

        static Command BuildCommand(DirectoryInfo workingDirectory, string[] arguments)
        {
            return Cli.Wrap(TestEnvironment.AppExecutable.FullName)
                .WithArguments(arguments)
                .WithWorkingDirectory(workingDirectory.FullName)
                .WithValidation(CommandResultValidation.None);
        }

        static void Log(int exitCode, string stdout, string stderr)
        {
            Console.WriteLine($"Exit code: {exitCode}\nStandard output: {stdout}\nStandard error: {stderr}");
        }
    }
}
