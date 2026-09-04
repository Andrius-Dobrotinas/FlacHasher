using System;
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
        // Interpolated rather than spelled out, so that the limit can't say one thing here and mean another in the check
        public static readonly string UsageText = $@"Usage: FakeDecoder <options>

  --file <path>              read bytes from this file
  --stdin                    read bytes from standard input
  --xor <hex-byte>           XOR every byte read with this value before writing it out (e.g. ""5A"")
  --expand <n>               write every byte read n times over, so the output outgrows the source (2 or more)
  --read-chunk-size <n>      bytes read from the source per read; a write is this many times --expand (default 4096)
  --write-delay <ms>         pause before each stdout write; -1 = wait forever
  --finish-after-reads <n>   leave the rest of the source unread and finish the run after n reads
  --progress-message <text>  written to stderr after each write
  --success-message <text>   written to stderr just before exit, when exit code is 0
  --error-message <text>     written to stderr just before exit, when exit code is non-zero
  --keep-stdout-open <ms>    wait with stdout still open before closing it; -1 = wait forever
  --linger <ms>              wait after closing stdout, before exiting; -1 = wait forever
  --exit-code <n>            exit code to return (default 0)

At most one source (--file or --stdin) may be given.
Without one, only --linger, --success-message, --error-message and --exit-code may be given: the rest have nothing to act on.
Each flag may be given at most once, and no value may be a flag name.
The buffers a read chunk and an expansion need together may not exceed {MaxBufferBytes / (1024 * 1024)} MiB.";

        public const int DefaultReadChunkSize = 4096;
        public const int MaxBufferBytes = 64 * 1024 * 1024;
        const int waitForever = -1;

        /// <summary>
        /// Nothing is read or written without a source, so none of these can do what it was given for.
        /// The ones left out - the exit message flags, --linger and --exit-code - all still have their effect on a sourceless run.
        /// </summary>
        static readonly string[] flagsNeedingASource =
        {
            "--xor",
            "--expand",
            "--read-chunk-size",
            "--write-delay",
            "--finish-after-reads",
            "--progress-message",
            "--keep-stdout-open"
        };

        public string SourceFile { get; private set; }
        public bool UseStdin { get; private set; }
        public byte? Xor { get; private set; }
        public int? Expand { get; private set; }
        public int ReadChunkSize { get; private set; } = DefaultReadChunkSize;
        public int? WriteDelayMs { get; private set; }
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

                // A flag name where a value belongs is a typo rather than a value, and swallowing it would leave a test running with arguments other than the ones it spells out
                if (value.StartsWith("--"))
                    return false;

                switch (flag)
                {
                    case "--file":
                        arguments.SourceFile = value;
                        break;
                    case "--xor":
                        // AllowHexSpecifier on its own: NumberStyles.HexNumber would also let surrounding whitespace through, which no flag value is meant to carry
                        if (!byte.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte xor))
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
                    case "--write-delay":
                        if (!TryParseInt(value, minValue: waitForever, out int writeDelayMs))
                            return false;
                        arguments.WriteDelayMs = writeDelayMs;
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
            if (arguments.SourceFile != null && !CanBeRead(arguments.SourceFile))
                return false;

            // Silence is the trap: a flag that can't take effect would leave the run looking like the one the test asked for while doing something else
            if (arguments.SourceFile == null && !arguments.UseStdin && flagsGiven.Overlaps(flagsNeedingASource))
                return false;

            // An expansion buys a second buffer alongside the read one, and both are allocated before a byte is read: a demand too big to meet would crash the run out of its requested exit code
            // The +1 is counted in long: at int.MaxValue it would otherwise wrap negative and let the very demand this guards against straight through
            long bufferBytes = (long)arguments.ReadChunkSize * (arguments.Expand != null ? (long)arguments.Expand.Value + 1 : 1L);
            if (bufferBytes > MaxBufferBytes)
                return false;

            result = arguments;
            return true;
        }

        static bool TryParseInt(string value, int minValue, out int result)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)
                && result >= minValue;
        }

        /// <summary>
        /// Opening it is the only answer to "can this be read?" that means anything - File.Exists says nothing about
        /// permissions or about someone else holding the file. The file is opened again for the actual reading, so one
        /// that turns unreadable in between still blows up; that's a surprise the run is better off dying of.
        /// All three exceptions say the same thing - this path is no good for reading - and which one turns up depends
        /// on the platform and on what's wrong with it: a permission denied is UnauthorizedAccessException on Unix and
        /// IOException on Windows, and a path too malformed to open at all, an empty one above all, is ArgumentException.
        /// Judging the shape of a path here instead would mean second-guessing, per platform, the very code about to open it.
        /// </summary>
        static bool CanBeRead(string path)
        {
            try
            {
                using (File.OpenRead(path))
                    return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
            {
                return false;
            }
        }
    }
}
