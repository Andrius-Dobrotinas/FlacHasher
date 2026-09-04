using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Andy.FakeDecoder
{
    /// <summary>
    /// Command-line arguments of the fake program.
    /// The parsing is hand-rolled on purpose, so that this test stub can't break because of a change in production code.
    /// </summary>
    public class Arguments
    {
        public const string UsageText = @"Usage: FakeDecoder <options>

  --file <path>              read bytes from this file
  --stdin                    read bytes from standard input
  --xor <hex-byte>           XOR every byte read with this value before writing it out (e.g. ""5A"")
  --expand <n>               write every byte read n times over, so the output outgrows the source (2 or more)
  --read-chunk-size <n>      bytes read from the source per read; a write is this many times --expand (default 4096)
  --output-chunk-delay <ms>  pause before each stdout write; -1 = wait forever
  --finish-after-reads <n>   leave the rest of the source unread and finish the run after n reads
  --progress-message <text>  written to stderr after each chunk is written
  --success-message <text>   written to stderr just before exit, when exit code is 0
  --error-message <text>     written to stderr just before exit, when exit code is non-zero
  --keep-stdout-open <ms>    wait with stdout still open before closing it; -1 = wait forever
  --linger <ms>              wait after closing stdout, before exiting; -1 = wait forever
  --exit-code <n>            exit code to return (default 0)

At most one source (--file or --stdin) may be given.
Each flag may be given at most once.
A read chunk multiplied by an expansion may not exceed 64 MiB.";

        public const int DefaultReadChunkSize = 4096;
        public const int MaxBufferBytes = 64 * 1024 * 1024;
        const int waitForever = -1;

        public string SourceFile { get; private set; }
        public bool UseStdin { get; private set; }
        public byte? Xor { get; private set; }
        public int? Expand { get; private set; }
        public int ReadChunkSize { get; private set; } = DefaultReadChunkSize;
        public int? OutputChunkDelayMs { get; private set; }
        public int? FinishAfterReads { get; private set; }
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
                    case "--expand":
                        if (!TryParseInt(value, minValue: 2, out int expand))
                            return false;
                        arguments.Expand = expand;
                        break;
                    case "--read-chunk-size":
                        if (!TryParseInt(value, minValue: 1, out int readChunkSize))
                            return false;
                        arguments.ReadChunkSize = readChunkSize;
                        break;
                    case "--output-chunk-delay":
                        if (!TryParseInt(value, minValue: waitForever, out int outputChunkDelayMs))
                            return false;
                        arguments.OutputChunkDelayMs = outputChunkDelayMs;
                        break;
                    case "--finish-after-reads":
                        if (!TryParseInt(value, minValue: 1, out int finishAfterReads))
                            return false;
                        arguments.FinishAfterReads = finishAfterReads;
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

            // The buffers are allocated before a byte is read, so a demand this size would crash the run out of its requested exit code
            if ((long)arguments.ReadChunkSize * (arguments.Expand ?? 1) > MaxBufferBytes)
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
