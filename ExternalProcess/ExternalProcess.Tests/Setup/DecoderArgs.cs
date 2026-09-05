using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Builds the fake decoder's arguments, rejecting here the combinations the program itself rejects.
    /// Left to reach the program, those come back as a usage error - exit code 1, empty stdout - which reads
    /// like a failure of the code under test rather than a mis-built run.
    /// </summary>
    class DecoderArgs
    {
        public const int WaitForever = -1;

        const int defaultReadChunkSize = 4096;
        const long maxBufferBytes = 64 * 1024 * 1024;

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

        readonly List<string> arguments = new List<string>();
        readonly HashSet<string> flagsGiven = new HashSet<string>();
        bool hasSource;
        int expansion = 1;
        int readChunkSize = defaultReadChunkSize;

        DecoderArgs()
        {
        }

        public static DecoderArgs Reading(FileInfo source)
        {
            var args = new DecoderArgs();
            args.hasSource = true;
            return args.Add("--file", source.FullName);
        }

        public static DecoderArgs ReadingStdin()
        {
            var args = new DecoderArgs();
            args.hasSource = true;
            return args.AddValueless("--stdin");
        }

        /// <summary>
        /// Nothing is read and stdout is never opened, so the consumer's EOF arrives with the exit rather than ahead of it.
        /// </summary>
        public static DecoderArgs WithoutSource()
        {
            return new DecoderArgs();
        }

        public DecoderArgs Xor(byte value) => Add("--xor", value.ToString("X2", CultureInfo.InvariantCulture));

        public DecoderArgs Expand(int factor)
        {
            Require(factor >= 2, "--expand has to be 2 or more; leave it off for output the same size as the source");
            expansion = factor;

            return Add("--expand", factor);
        }

        public DecoderArgs ReadChunkSize(int bytes)
        {
            Require(bytes >= 1, "--read-chunk-size has to be 1 or more");
            readChunkSize = bytes;

            return Add("--read-chunk-size", bytes);
        }

        public DecoderArgs WriteDelay(int milliseconds) => AddWait("--write-delay", milliseconds);

        public DecoderArgs FinishAfterReads(int reads)
        {
            Require(reads >= 1, "--finish-after-reads has to be 1 or more");

            return Add("--finish-after-reads", reads);
        }

        public DecoderArgs ProgressMessage(string text) => Add("--progress-message", text);

        public DecoderArgs SuccessMessage(string text) => Add("--success-message", text);

        public DecoderArgs ErrorMessage(string text) => Add("--error-message", text);

        public DecoderArgs KeepStdoutOpen(int milliseconds) => AddWait("--keep-stdout-open", milliseconds);

        public DecoderArgs Linger(int milliseconds) => AddWait("--linger", milliseconds);

        public DecoderArgs ExitCode(int code) => Add("--exit-code", code);

        public string[] Build()
        {
            if (!hasSource)
            {
                var unusable = flagsGiven.Intersect(flagsNeedingASource).ToArray();
                Require(!unusable.Any(), $"{string.Join(", ", unusable)} can't take effect without a source");
            }

            // Both buffers are allocated before a byte is read, so a demand too big to meet costs the run its exit code
            long bufferBytes = (long)readChunkSize * (expansion == 1 ? 1L : expansion + 1L);
            Require(bufferBytes <= maxBufferBytes, $"a read chunk and its expansion need {bufferBytes} bytes together, past the {maxBufferBytes} allowed");

            return arguments.ToArray();
        }

        DecoderArgs Add(string flag, int value) => Add(flag, value.ToString(CultureInfo.InvariantCulture));

        DecoderArgs Add(string flag, string value)
        {
            // A flag name where a value belongs is taken for a typo and rejected, so it can't be passed on as one
            Require(!value.StartsWith("--"), $"the value of {flag} can't start with '--'");

            AddValueless(flag);
            arguments.Add(value);

            return this;
        }

        DecoderArgs AddValueless(string flag)
        {
            Require(flagsGiven.Add(flag), $"{flag} has been given already");
            arguments.Add(flag);

            return this;
        }

        DecoderArgs AddWait(string flag, int milliseconds)
        {
            Require(milliseconds >= WaitForever, $"{flag} has to be {WaitForever} (forever) or more");

            return Add(flag, milliseconds);
        }

        static void Require(bool condition, string message)
        {
            if (!condition)
                throw new ArgumentException($"The fake decoder would reject these arguments: {message}");
        }
    }
}
