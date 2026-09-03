using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Andy.FakeCmdline
{
    /// <summary>
    /// Command-line arguments of the fake program.
    /// The parsing is hand-rolled on purpose, so that this test stub can't break because of a change in production code.
    /// </summary>
    public class Arguments
    {
        public const string UsageText = @"Usage: FakeCmdline <options>

  --file <path>              read bytes from this file
  --stdin                    read bytes from standard input
  --xor <hex-byte>           XOR every byte read with this value before writing it out (e.g. ""5A"")
  --output-chunk-size <n>    bytes per stdout write (default 4096)
  --output-chunk-delay <ms>  pause before each stdout write; -1 = wait forever
  --stop-after-chunks <n>    give up on the rest of the source after n writes
  --progress-message <text>  written to stderr after each chunk is written
  --success-message <text>   written to stderr just before exit, when exit code is 0
  --error-message <text>     written to stderr just before exit, when exit code is non-zero
  --keep-stdout-open <ms>    wait with stdout still open before closing it; -1 = wait forever
  --linger <ms>              wait after closing stdout, before exiting; -1 = wait forever
  --exit-code <n>            exit code to return (default 0)

At most one source (--file or --stdin) may be given.
Each flag may be given at most once.";

        public const int DefaultOutputChunkSize = 4096;
        const int waitForever = -1;

        public string SourceFile { get; private set; }
        public bool UseStdin { get; private set; }
        public byte? Xor { get; private set; }
        public int OutputChunkSize { get; private set; } = DefaultOutputChunkSize;
        public int? OutputChunkDelayMs { get; private set; }
        public int? StopAfterChunks { get; private set; }
        public string ProgressMessage { get; private set; }
        public string SuccessMessage { get; private set; }
        public string ErrorMessage { get; private set; }
        public int? KeepStdoutOpenMs { get; private set; }
        public int? LingerMs { get; private set; }
        public int ExitCode { get; private set; }

        public static bool TryParse(string[] args, out Arguments result)
        {
            result = null;

            var arguments = new Arguments();
            var flagsGiven = new HashSet<string>();
            for (int i = 0; i < args.Length; i++)
            {
                string flag = args[i];

                // Letting the last one win would leave a test quietly running with arguments other than the ones it spells out
                if (!flagsGiven.Add(flag))
                    return false;

                if (flag == "--stdin")
                {
                    arguments.UseStdin = true;
                    continue;
                }

                if (i + 1 == args.Length)
                    return false;

                string value = args[++i];
                switch (flag)
                {
                    case "--file":
                        arguments.SourceFile = value;
                        break;
                    case "--xor":
                        if (!byte.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte xor))
                            return false;
                        arguments.Xor = xor;
                        break;
                    case "--output-chunk-size":
                        if (!TryParseInt(value, minValue: 1, out int outputChunkSize))
                            return false;
                        arguments.OutputChunkSize = outputChunkSize;
                        break;
                    case "--output-chunk-delay":
                        if (!TryParseInt(value, minValue: waitForever, out int outputChunkDelayMs))
                            return false;
                        arguments.OutputChunkDelayMs = outputChunkDelayMs;
                        break;
                    case "--stop-after-chunks":
                        if (!TryParseInt(value, minValue: 1, out int stopAfterChunks))
                            return false;
                        arguments.StopAfterChunks = stopAfterChunks;
                        break;
                    case "--progress-message":
                        arguments.ProgressMessage = value;
                        break;
                    case "--success-message":
                        arguments.SuccessMessage = value;
                        break;
                    case "--error-message":
                        arguments.ErrorMessage = value;
                        break;
                    case "--keep-stdout-open":
                        if (!TryParseInt(value, minValue: waitForever, out int keepStdoutOpenMs))
                            return false;
                        arguments.KeepStdoutOpenMs = keepStdoutOpenMs;
                        break;
                    case "--linger":
                        if (!TryParseInt(value, minValue: waitForever, out int lingerMs))
                            return false;
                        arguments.LingerMs = lingerMs;
                        break;
                    case "--exit-code":
                        if (!TryParseInt(value, minValue: int.MinValue, out int exitCode))
                            return false;
                        arguments.ExitCode = exitCode;
                        break;
                    default:
                        return false;
                }
            }

            if (arguments.SourceFile != null && arguments.UseStdin)
                return false;

            // Caught here rather than left to blow up when opening the file: an unhandled exception would replace the requested exit code with a runtime-chosen one
            if (arguments.SourceFile != null && !File.Exists(arguments.SourceFile))
                return false;

            // Without a source there's no stdout writing to hold off, so the wait would be silently ignored
            if (arguments.KeepStdoutOpenMs != null && arguments.SourceFile == null && !arguments.UseStdin)
                return false;

            result = arguments;
            return true;
        }

        static bool TryParseInt(string value, int minValue, out int result)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)
                && result >= minValue;
        }
    }
}
