using CliWrap;
using CliWrap.Buffered;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    record AppResult(int ExitCode, string StdOut, string StdErr);

    static class App
    {
        public static async Task<AppResult> Run(DirectoryInfo workingDirectory, TimeSpan timeout, params string[] arguments)
        {
            using var cancellation = new CancellationTokenSource(timeout);

            var result = await Cli.Wrap(TestEnvironment.AppExecutable.FullName)
                .WithArguments(arguments)
                .WithWorkingDirectory(workingDirectory.FullName)
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync(cancellation.Token);

            Console.WriteLine($"Exit code: {result.ExitCode}\nStandard output: {result.StandardOutput}\nStandard error: {result.StandardError}");

            return new AppResult(result.ExitCode, result.StandardOutput, result.StandardError);
        }

        public static async Task<AppResult> Run(DirectoryInfo workingDirectory, params string[] arguments)
        {
            return await Run(workingDirectory, TimeSpan.FromMinutes(1), arguments);
        }
    }
}
