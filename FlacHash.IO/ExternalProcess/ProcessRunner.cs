using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.FlacHash.ExternalProcess
{
    public class ProcessRunner : IIOProcessRunner, IOutputOnlyProcessRunner
    {
        private readonly int exitTimeoutMs;
        private readonly int startDelayMs = 100;
        private readonly int timeoutMs;
        private readonly bool showProcessWindowWithStdErrOutput;

        public const int NoTimeoutValue = -1;

        /// <param name="timeoutSec">If a process doesn't finish within a given time (in seconds), it will be termined without returning any result</param>
        /// <param name="exitTimeoutMs">Time to wait (in milliseconds) for the process to exit after all of its stdout has been read. Shouldn't be a large value because most processes exit right after finishing to write to stdout.</param>
        /// <param name="startDelayMs">Time to wait (in milliseconds) before starting interacting with the process' std streams.
        /// Sometimes (depending on the speed of the computer?) it doesn't have std streams available right away, which results in "Pipe ended" error.</param>
        /// <param name="showProcessWindowWithStdErrOutput">When on, doesn't capture the process' stderror and therefore can't report errors - but the info is there for the user to see in window.
        /// When off, captures stderr and includes in exceptions if the process fails</param>
        public ProcessRunner(int timeoutSec, int exitTimeoutMs, int startDelayMs, bool showProcessWindowWithStdErrOutput)
        {
            this.timeoutMs = timeoutSec == NoTimeoutValue ? NoTimeoutValue : timeoutSec * 1000;
            this.exitTimeoutMs = exitTimeoutMs;
            this.startDelayMs = startDelayMs;
            this.showProcessWindowWithStdErrOutput = showProcessWindowWithStdErrOutput;
        }

        public Stream RunAndReadOutput(
            FileInfo executableFile,
            IEnumerable<string> arguments,
            CancellationToken cancellation = default)
        {
            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(executableFile, arguments, showProcessWindowWithStdErrOutput);
            
            using (var process = new Process { StartInfo = processSettings })
            {
                process.Start();

                return GetOutputStream(process, cancellation);
            }
        }

        /* The idea is:
        * -start the process
        * -start reading std streams (can't start writing/reading to/from them until the process is running)
        * -wait until the process produces the output (std-out is fully read)
        * -get process exit code and error info/ if applicable
        * 
        * In addition to this, it also listens to cancellations and for time-out, which forces the killing of the process
        */
        public Stream RunAndReadOutput(
            FileInfo executableFile,
            IEnumerable<string> arguments,
            Stream inputData,
            CancellationToken cancellation = default)
        {
            if (inputData == null) throw new ArgumentNullException(nameof(inputData));

            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(executableFile, arguments, showProcessWindowWithStdErrOutput);
            processSettings.RedirectStandardInput = true;

            using (var process = new Process { StartInfo = processSettings })
            {
                process.Start();

                Task.Delay(startDelayMs).GetAwaiter().GetResult(); //throws a "Pipe ended" error when trying to write to std right away. Waiting a bit before writing seems to solve the problem, but this could be problematic if the system is slower...

                var writeTask = Task.Run(() => WriteToStdIn(process, inputData));

                return GetOutputStream(process, cancellation);
            }
        }

        private MemoryStream GetOutputStream(Process process, CancellationToken cancellation = default)
        {
            //Error (progress) stream has to be actively read as when the buffer fills up, the process stops writing to std-out.
            //The bigger the file, the more is written to the error stream as progress report
            CancellationTokenSource errorReadCancellation = null;
            Task<MemoryStream> stdErrorTask = null;
            if (process.StartInfo.RedirectStandardError)
            {
                errorReadCancellation = new CancellationTokenSource();
                stdErrorTask = Task.Run<MemoryStream>(() => ReadStreamCancellable(process.StandardError.BaseStream, errorReadCancellation.Token));
            }

            //the writing operation gets blocked until someone reads the output - when std-out is redirected.
            var outputReadTask = Task.Run<MemoryStream>(() => ReadStream(process.StandardOutput.BaseStream));

            //wait for reading to finish or terminate the process on time-out or cancellation
            bool timedOut;
            try
            {
                var finishedInTime = Task.WaitAll(
                    new[] { outputReadTask },
                    timeoutMs,
                    cancellation);

                timedOut = !finishedInTime;
            }
            catch (OperationCanceledException)
            {
                process.Kill(true);
                throw;
            }

            if (timedOut)
            {
                //just in case it just finished
                if (!process.HasExited)
                {
                    process.Kill(true);
                    throw new TimeoutException("The process has taken longer than allowed and has been cancelled");
                }
            }
         
            //At this point, the std-out-read task is successfully finished and will not block.
            //It hasn't been cancelled, and hasn't timed-out - so there will be no exception.
            //Even if time-out had fired, it must've still finished before it got around to killing the process.
            var outputStream = outputReadTask.GetAwaiter().GetResult();

            ProcessExitCode(process, exitTimeoutMs, stdErrorTask, errorReadCancellation);

            return outputStream;
        }

        private static void WriteToStdIn(Process process, Stream inputData)
        {
            var stdin = process.StandardInput.BaseStream;
            inputData.CopyTo(stdin);

            //it looks like either the stream has to be closed, or an "end of file" char (-1 in int language) must be written to the stream
            stdin.Close();
        }

        private static MemoryStream ReadStream(Stream outputStream)
        {
            var processOutput = new MemoryStream();
            outputStream.CopyTo(processOutput);
            
            processOutput.Seek(0, SeekOrigin.Begin);
            return processOutput;
        }

        private static MemoryStream ReadStreamCancellable(Stream outputStream, CancellationToken cancellation = default)
        {
            var processOutput = new MemoryStream();

            //Just in case the stream freezes even after process exits (unlikely, but I've heard about such a problem).
            //Whatever has been copied over, should be returned because it can be useful even if it's incomplete.
            try
            {
                outputStream.CopyToAsync(processOutput, cancellation).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }

            processOutput.Seek(0, SeekOrigin.Begin);
            return processOutput;
        }

        private static void ProcessExitCode(Process process, int exitTimeoutMs, Task<MemoryStream> stdErrorTask = null, CancellationTokenSource errorReadCancellation = null)
        {
            //sometimes it takes the process a while to quit after closing the std-out
            if (process.WaitForExit(exitTimeoutMs) == false)
                process.Kill(true);

            if (process.ExitCode != 0)
            {
                if (stdErrorTask == null)
                    throw new ExecutionException(process.ExitCode);

                try
                {
                    // Wait for std-error read task to finish - or force-stop it on time-out
                    using (var stdError = WaitForErrorStream(stdErrorTask, exitTimeoutMs, errorReadCancellation))
                    {
                        string processErrorOutput = ReadErrorStreamOutput(stdError);
                        throw new ExecutionException(process.ExitCode, processErrorOutput);
                    }
                }
                catch (ExecutionException)
                {
                    throw;
                }
                catch
                {
                    throw new ExecutionException(process.ExitCode);
                }
            }
        }

        private static TStream WaitForErrorStream<TStream>(Task<TStream> stdErrorTask, int exitTimeoutMs, CancellationTokenSource errorReadCancellation)
        {
            bool finished = stdErrorTask.Wait(exitTimeoutMs);
            
            //Shouldn't ever happen, but just in case the stars align right
            if (!finished)
                errorReadCancellation.Cancel();

            //Even if cancelled, it will have captured stuff (anything up to cancellation)
            return stdErrorTask.GetAwaiter().GetResult();
        }

        private static string ReadErrorStreamOutput(Stream stdError)
        {
            using (var reader = new StreamReader(stdError))
            {
                return reader.ReadToEnd();
            }
        }
    }
}