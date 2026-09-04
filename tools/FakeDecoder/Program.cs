using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Andy.FakeDecoder
{
    /// <summary>
    /// A fake external program for tests of process running. It reads bytes from a source, writes them to stdout,
    /// reports progress on stderr, and can be told to fail, stall or linger.
    /// </summary>
    class Program
    {
        const int usageErrorExitCode = 1;

        static int Main(string[] args)
        {
            if (!Arguments.TryParse(args, out Arguments arguments))
            {
                // Usage goes to stderr, so that a test with a mistyped flag sees an empty stdout instead of help text
                WriteLineToStdErr(Arguments.UsageText);
                return usageErrorExitCode;
            }

            using (Stream source = OpenSource(arguments))
            {
                if (source == null)
                {
                    WriteLineToStdErr("Nothing to process");

                    // Run bare, this is a human looking for the flag list rather than a test driving it
                    if (args.Length == 0)
                        WriteLineToStdErr(Arguments.UsageText);
                }
                else
                {
                    // Raw stream only: text writers apply encoding and newline translation, which corrupts binary payloads
                    Stream stdout = OpenStdout();

                    WriteChunks(source, stdout, arguments);

                    if (arguments.KeepStdoutOpenMs != null)
                        // Thread.Sleep(-1) is Timeout.Infinite, so "wait forever" needs no special handling
                        Thread.Sleep(arguments.KeepStdoutOpenMs.Value);

                    // The consumer only gets an EOF once the process lets go of stdout
                    CloseStdout(stdout);
                }
            }

            if (arguments.LingerMs != null)
                Thread.Sleep(arguments.LingerMs.Value);

            string exitMessage = arguments.ExitCode == 0 ? arguments.SuccessMessage : arguments.ErrorMessage;
            if (exitMessage != null)
                WriteLineToStdErr(exitMessage);

            return arguments.ExitCode;
        }

        static Stream OpenSource(Arguments arguments)
        {
            if (arguments.SourceFile != null)
                return File.OpenRead(arguments.SourceFile);

            if (arguments.UseStdin)
                return OpenStandardStream(stdInputHandle, unixDescriptor: 0, FileAccess.Read, ownsHandle: false);

            return null;
        }

        /// <summary>
        /// Opens standard output as a stream that **owns the handle**, so that disposing of it really does close it.
        /// Console.OpenStandardOutput() is no good for this: on Windows the stream it returns leaves the process'
        /// standard handle alone when disposed of, and on Unix it wraps a dup() of descriptor 1, so disposing of it
        /// closes the copy and not the original. Either way a write end of the pipe stays open and the consumer sees
        /// no EOF until the process exits, which would make --keep-stdout-open and --linger indistinguishable.
        /// </summary>
        static Stream OpenStdout()
        {
            return OpenStandardStream(stdOutputHandle, unixDescriptor: 1, FileAccess.Write, ownsHandle: true);
        }

        /// <summary>
        /// Standard output is the only one of the three this program ever has to close early, so it's the only one
        /// opened with <paramref name="ownsHandle"/> on.
        /// </summary>
        static Stream OpenStandardStream(int windowsHandle, int unixDescriptor, FileAccess access, bool ownsHandle)
        {
            var handle = new SafeFileHandle(
                OperatingSystem.IsWindows() ? GetStdHandle(windowsHandle) : new IntPtr(unixDescriptor),
                ownsHandle);

            return new FileStream(handle, access);
        }

        /// <summary>
        /// Lets go of standard output for good, which is what hands the consumer its EOF.
        /// On Unix, disposing of the stream releases descriptor 1 only, and the runtime leaves duplicates of the
        /// standard descriptors behind while starting up - a pipe stays open for as long as any descriptor still
        /// points at it. Steering clear of System.Console does not prevent those duplicates; they have to be closed.
        /// Windows has no such problem
        /// </summary>
        static void CloseStdout(Stream stdout)
        {
            if (!OperatingSystem.IsLinux())
            {
                DisposeOfStdout(stdout);
                return;
            }
            else
            {
                var duplicates = FindLinuxDuplicatesOfStdout();

                DisposeOfStdout(stdout);

                foreach (int descriptor in duplicates)
                    new SafeFileHandle(new IntPtr(descriptor), ownsHandle: true).Dispose();
            }
        }

        /// <summary>
        /// A stream still holds on to the bytes of a write that failed and writes them out again when it's disposed of,
        /// so a pipe the consumer has abandoned breaks a second time here. It's the same non-failure as the first one
        /// (see WriteChunks) and must not cost the program the exit code it was asked for either; the handle is
        /// released regardless of the flush going wrong.
        /// </summary>
        static void DisposeOfStdout(Stream stdout)
        {
            try
            {
                stdout.Dispose();
            }
            catch (IOException)
            {
            }
        }

        /// <summary>
        /// Reading the process' own descriptor table this way is a Linux facility - macOS has no /proc and would
        /// need a different technique, so the stdout-closing behaviour wouldn't hold there. Nothing is run on macOS,
        /// so it has never been needed and is untested.
        /// </summary>
        static int[] FindLinuxDuplicatesOfStdout()
        {
            // Has to be read while descriptor 1 is still open - afterwards there's no telling what it used to point at
            string stdoutTarget = ReadDescriptorTarget("1");
            string stderrTarget = ReadDescriptorTarget("2");

            // Standard output and error are the same stream here, and closing one would take the other down with it
            if (stdoutTarget == null || stdoutTarget == stderrTarget)
                return Array.Empty<int>();

            return Directory.EnumerateFileSystemEntries(linuxDescriptorDirectory)
                .Select(Path.GetFileName)
                // Standard input and error aren't this method's to close, whatever they point at
                .Where(name => int.TryParse(name, out int descriptor) && descriptor > 2)
                .Where(name => ReadDescriptorTarget(name) == stdoutTarget)
                .Select(int.Parse)
                .ToArray();
        }

        static string ReadDescriptorTarget(string descriptor)
        {
            return new FileInfo(Path.Combine(linuxDescriptorDirectory, descriptor)).LinkTarget;
        }

        const string linuxDescriptorDirectory = "/proc/self/fd";
        const int stdInputHandle = -10;
        const int stdOutputHandle = -11;
        const int stdErrorHandle = -12;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        static void WriteChunks(Stream source, Stream stdout, Arguments arguments)
        {
            int expansion = arguments.Expand ?? 1;
            var buffer = new byte[arguments.ReadChunkSize];
            // Without expansion the bytes go out exactly as they were read, so there's nothing for a second buffer to hold
            var expandedBuffer = arguments.Expand != null ? new byte[buffer.Length * expansion] : null;
            int readsMade = 0;

            while (true)
            {
                int byteCount = source.Read(buffer, 0, buffer.Length);
                if (byteCount == 0)
                    return;

                readsMade++;

                if (arguments.OutputChunkDelayMs != null)
                    Thread.Sleep(arguments.OutputChunkDelayMs.Value);

                var output = expandedBuffer ?? buffer;
                int outputByteCount = expandedBuffer != null
                    ? Expand(buffer, byteCount, expandedBuffer, expansion)
                    : byteCount;

                if (arguments.Xor != null)
                    for (int i = 0; i < outputByteCount; i++)
                        output[i] ^= arguments.Xor.Value;

                try
                {
                    stdout.Write(output, 0, outputByteCount);
                    // Flushing every chunk: buffering would otherwise merge the chunks and destroy the timing the tests observe
                    stdout.Flush();
                }
                catch (IOException)
                {
                    // The consumer let go of the read end. Its going away is not this program's failure to report, so the
                    // writing just stops and the run carries on to the exit routine with the exit code it was asked for
                    return;
                }

                if (arguments.ProgressMessage != null)
                    WriteLineToStdErr(arguments.ProgressMessage);

                if (readsMade == arguments.FinishAfterReads)
                    return;
            }
        }

        /// <summary>
        /// Repeats every byte, which is what makes the output bigger than the source, the way a real decoder's is.
        /// </summary>
        static int Expand(byte[] source, int byteCount, byte[] destination, int factor)
        {
            for (int i = 0; i < byteCount; i++)
                for (int repeat = 0; repeat < factor; repeat++)
                    destination[i * factor + repeat] = source[i];

            return byteCount * factor;
        }

        static void WriteLineToStdErr(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);
            stderr.Write(bytes, 0, bytes.Length);
            stderr.Flush();
        }

        static readonly Stream stderr = OpenStandardStream(stdErrorHandle, unixDescriptor: 2, FileAccess.Write, ownsHandle: false);
    }
}
