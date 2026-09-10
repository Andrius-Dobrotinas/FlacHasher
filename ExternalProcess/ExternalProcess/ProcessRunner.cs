using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.ExternalProcess
{
    public class ProcessRunner : IIOProcessRunner, IOutputOnlyProcessRunner
    {
        private readonly int exitTimeoutMs;
        private readonly int startWaitMs;
        private readonly int timeoutMs;
        private readonly int maxErrorOutputBytes;
        private readonly bool showProcessOutput;

        private const int ExitCode_CtrlC_Windows = -1073741510;
        private const int ExitCode_CtrlC_Unix = 130; // SIGINT, reported as 128 + the signal number

        /// <summary>
        /// Unix keeps only the low byte of an exit status, so the Windows value can never turn up there.
        /// </summary>
        private static int ExitCode_CtrlC => OperatingSystem.IsWindows() ? ExitCode_CtrlC_Windows : ExitCode_CtrlC_Unix;

        public const int NoTimeoutValue = -1;
        public const int UnboundedErrorOutput = ErrorOutputBuffer.Unbounded;
        public const int DefaultMaxErrorOutputBytes = 64 * 1024;

        /// <summary>
        /// The process timeout is configured in seconds but applied in milliseconds; this is the one place that knows both.
        /// </summary>
        public static int TimeoutFromSeconds(int timeoutSec)
        {
            return timeoutSec == NoTimeoutValue ? NoTimeoutValue : timeoutSec * 1000;
        }

        /// <param name="timeoutMs">If a process doesn't finish within a given time (in milliseconds), it will be termined without returning any result. <see cref="NoTimeoutValue"/> for no timeout</param>
        /// <param name="exitTimeoutMs">Time to wait (in milliseconds) for the process to exit after all of its stdout has been read. Shouldn't be a large value because most processes exit right after finishing to write to stdout.</param>
        /// <param name="startWaitMs">Time to wait (in milliseconds) before starting interacting with the process' std streams.
        /// Sometimes (depending on the speed of the computer?) it doesn't have std streams available right away, which results in "Pipe ended" error.
        /// Only applies when the process is being fed through its standard input, which is the only thing that has ever run into it.</param>
        /// <param name="maxErrorOutputBytes">How much of the process' error output to keep for reporting a failure with. The most recent bytes are the ones kept.
        /// <see cref="UnboundedErrorOutput"/> to keep all of it, at the cost of a buffer that grows with the run</param>
        /// <param name="showProcessOutput">When on, doesn't capture the process' stderror and therefore can't report errors - but the info is there for the user to see in window.
        /// When off, captures stderr and includes in exceptions if the process fails</param>
        public ProcessRunner(int timeoutMs, int exitTimeoutMs, int startWaitMs, int maxErrorOutputBytes, bool showProcessOutput)
        {
            this.timeoutMs = timeoutMs;
            this.exitTimeoutMs = exitTimeoutMs;
            this.startWaitMs = startWaitMs;
            this.maxErrorOutputBytes = maxErrorOutputBytes;
            this.showProcessOutput = showProcessOutput;
        }

        public ProcessOutputStream RunAndReadOutput(
            FileInfo executableFile,
            IEnumerable<string> arguments,
            CancellationToken cancellation = default)
        {
            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(executableFile, arguments, showProcessOutput);

            var process = new ExternalProcess { StartInfo = processSettings };

            return GetOutputStream_WaitProcessExitInParallel(process, input: null, process.StartInfo.RedirectStandardError, cancellation);
        }

        /* The idea is:
        * -start the process
        * -start reading std streams (can't start writing/reading to/from them until the process is running)
        * -wait until the process produces the output (std-out is fully read)
        * -get process exit code and error info/ if applicable
        * 
        * In addition to this, it also listens to cancellations and for time-out, which forces the killing of the process
        */
        public ProcessOutputStream RunAndReadOutput(
            FileInfo executableFile,
            IEnumerable<string> arguments,
            Stream inputData,
            CancellationToken cancellation = default)
        {
            if (inputData == null) throw new ArgumentNullException(nameof(inputData));

            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(executableFile, arguments, showProcessOutput);
            processSettings.RedirectStandardInput = true;

            var process = new ExternalProcess { StartInfo = processSettings };

            return GetOutputStream_WaitProcessExitInParallel(process, inputData, process.StartInfo.RedirectStandardError, cancellation);
        }

        /// <summary>
        /// Not part of the contract: the two <see cref="RunAndReadOutput"/> overloads are. This exists so that the
        /// few behaviours no real process can be made to produce can be staged with a fake one.
        /// </summary>
        internal ProcessOutputStream GetOutputStream_WaitProcessExitInParallel(IExternalProcess process, Stream input = null, bool readStderr = false, CancellationToken cancellation = default)
        {
            try
            {
                process.Start();
            }
            catch (Win32Exception e)
            {
                throw new LaunchException(e);
            }

            /* Only a process being fed has ever needed this: it's writing to a process that isn't ready to receive yet that fails.
             * Draining its error stream starts here too, so the wait covers that as well.
             * A process with nothing to receive has nothing to be too early for, and waiting on it is dead time on every run. */
            if (input != null)
                Task.Delay(startWaitMs).GetAwaiter().GetResult();

            /* Error (progress) stream has to be actively read as when the buffer fills up, the process stops writing to std-out
             * (probably depends on whether stderr and stdout writes sequence or in parallel in the program).
             * The bigger the input file, the more is written to the error stream as progress report */
            CancellationTokenSource errorReadCancellation = null;
            ErrorOutputBuffer errorOutput = null;
            Task stdErrorTask = null;
            Task inputTask = null;
            InputDelivery inputDelivery = null;
            if (readStderr)
            {
                errorReadCancellation = new CancellationTokenSource();
                errorOutput = new ErrorOutputBuffer(maxErrorOutputBytes);
                stdErrorTask = BackgroundTask.StartBackgroundTask(() => ReadErrorOutput(process.StandardError.BaseStream, errorOutput, errorReadCancellation.Token));
            }

            if (input != null)
            {
                inputDelivery = new InputDelivery();
                inputTask = BackgroundTask.StartBackgroundTask(() => WriteToStdInAndDisposeOf(process.StandardInput.BaseStream, input, inputDelivery));
            }

            //I don't need a return value, but there's no non-generic version of this
            var outputReadTaskCompletion = new TaskCompletionSource<object>();

            var processWaitTask = BackgroundTask.StartBackgroundTask(() =>
            {
                WaitForOutputRead_AndProcessExitCode(process, outputReadTaskCompletion.Task, stdErrorTask, errorOutput, cancellation, errorReadCancellation, inputTask, inputDelivery);
            });

            return new ProcessOutputStream(process.StandardOutput.BaseStream, outputReadTaskCompletion, processWaitTask);
        }

        /// <summary>
        /// Waits for output or error task to finish and processes the exit code.
        /// Also, handles cancellation and time-out.
        /// At the end, disposes of <paramref name="process"/>
        /// </summary>
        private void WaitForOutputRead_AndProcessExitCode(IExternalProcess process, Task outputReadTask, Task stdErrorTask = null, ErrorOutputBuffer errorOutput = null, CancellationToken cancellation = default, CancellationTokenSource errorReadCancellation = null, Task inputTask = null, InputDelivery inputDelivery = null)
        {
            try
            {
                //wait for reading to finish or terminate the process on time-out or cancellation
                bool timedOut;
                try
                {
                    try
                    {
                        var finishedInTime = Task.WaitAll(
                            new[] { outputReadTask },
                            timeoutMs,
                            cancellation);

                        timedOut = !finishedInTime;
                    }
                    // This gets throw by WaitAll when one of the tasks it's waiting gets cancelled
                    // When WaitAll gets cancelled (via the cancellation token), it throws OperationCancelledException
                    catch (AggregateException e)
                    {
                        // Simply to unwrap the aggregate exception
                        throw e.InnerException;
                    }
                }
                catch (OperationCanceledException)
                {
                    // When exiting/getting killed, the process (normally) sends EOF to stdout and closes all streams 
                    if (!process.HasExited)
                        process.Kill(true);

                    throw new OperationCanceledException("Process has been cancelled");
                }

                if (timedOut)
                {
                    //just in case it just finished
                    if (!process.HasExited)
                    {
                        // When exiting/getting killed, the process (normally) sends EOF to stdout and closes all streams 
                        process.Kill(true);

                        throw new ProcessTimeoutException(
                            HarvestErrorOutput(stdErrorTask, errorOutput, exitTimeoutMs),
                            isProcessOutputCaptured: errorOutput != null);
                    }
                    // The process exited before there was a chance to kill it (lucky)
                }

                // At this point, the std-out-read task is successfully finished and will not block.
                // It hasn't been cancelled, and hasn't timed-out.
                // Even if time-out had fired, it must've still finished before it got around to killing the process.

                ProcessExitCode(process, exitTimeoutMs, stdErrorTask, errorOutput, inputTask, inputDelivery);
            }
            finally
            {
                /* Disposing of a Process leaves its streams alone, deliberately, so that a consumer can go on reading
                 * one after the process is gone. Nothing else lets go of the error stream either - stdout belongs to
                 * the stream handed to the caller, and stdin to whoever finished feeding it.
                 * Asking for a stream that was never redirected throws, so only the one that was gets released. */
                errorReadCancellation?.Cancel();
                errorReadCancellation?.Dispose();

                if (errorOutput != null)
                    Release(process.StandardError);

                process.Dispose();
            }
        }

        /// <summary>
        /// A reader or writer still working on one of these ends its work on a closed stream, which it treats as the
        /// end of the line rather than a fault - by this point there is nothing left for either of them to do.
        /// </summary>
        private static void Release(IDisposable stream)
        {
            try
            {
                stream?.Dispose();
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private static void WriteToStdInAndDisposeOf(Stream target, Stream inputData, InputDelivery delivery)
        {
            try
            {
                //This errors out when the process gets closed prematurely (timeout/cancellation) due to stdin getting closed/disposed of
                inputData.CopyTo(target);

                delivery.MarkFinished();
            }
            // The process let go of the read end before it had everything. Nobody is waiting on this task,
            // so the failure is recorded by the delivery going unmarked rather than by an exception no one would see.
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                //it looks like either the stream has to be closed, or an "end of file" char (-1 in int language) must be written to the stream
                Release(target);
                inputData.Dispose();
            }
        }

        /// <summary>
        /// Drains the process' error output for as long as there is any, keeping the tail of it.
        /// Draining has to continue past the cap: a full pipe stops the process writing, and it writes to stdout through the same stalls.
        /// </summary>
        private static void ReadErrorOutput(Stream errorStream, ErrorOutputBuffer buffer, CancellationToken cancellation)
        {
            var chunk = new byte[4096];

            /* Whatever has been collected stays usable even if the reading ends badly, so every way out of here is quiet.
             * On Unix, closing a pipe under a blocked read surfaces as an IOException rather than an end of stream. */
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    int count = errorStream.Read(chunk, 0, chunk.Length);
                    if (count == 0)
                        return;

                    buffer.Write(chunk, count);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private static void ProcessExitCode(IExternalProcess process, int exitTimeoutMs, Task stdErrorTask = null, ErrorOutputBuffer errorOutput = null, Task inputTask = null, InputDelivery inputDelivery = null)
        {
            //sometimes it takes the process a while to quit after closing the std-out
            if (process.WaitForExit(exitTimeoutMs) == false)
            {
                process.Kill(true);

                /* All of the output has been served by this point, but nothing has vouched for it.
                 * The exit code of a killed process is the one the OS terminated it with, never the one the program
                 * would have chosen, so there is nothing here to tell a finished job from an abandoned one. */
                throw new ProcessNotRespondingException(
                    HarvestErrorOutput(stdErrorTask, errorOutput, exitTimeoutMs),
                    isProcessOutputCaptured: errorOutput != null);
            }

            if (process.ExitCode != 0)
            {
                // This happens when this is run by a cmd-line application and it gets Ctrl+C'd as it relays the command to the spawned process
                if (process.ExitCode == ExitCode_CtrlC)
                    throw new OperationCanceledException("Process has been cancelled");

                if (errorOutput == null)
                    throw new ExecutionWithExitCodeException(process.ExitCode);

                throw new ExecutionWithExitCodeException(
                    process.ExitCode,
                    HarvestErrorOutput(stdErrorTask, errorOutput, exitTimeoutMs),
                    isProcessOutputCaptured: true);
            }

            /* Last of all: an exit code the process chose for itself outranks this, and every other way a run can end
             * kills the process, which breaks the feeding as a matter of course rather than as a fault of its own. */
            if (inputDelivery != null && !FinishedDelivering(inputTask, inputDelivery, exitTimeoutMs))
            {
                throw new PrematureExitException(
                    process.ExitCode,
                    HarvestErrorOutput(stdErrorTask, errorOutput, exitTimeoutMs),
                    isProcessOutputCaptured: errorOutput != null);
            }
        }

        /// <summary>
        /// The process closing its input is what lets it exit, so by now the feeding has all but certainly finished
        /// one way or the other; the wait is only to settle the last of the race.
        /// </summary>
        private static bool FinishedDelivering(Task inputTask, InputDelivery inputDelivery, int exitTimeoutMs)
        {
            try
            {
                inputTask?.Wait(exitTimeoutMs);
            }
            catch
            {
                // Whether it finished is recorded on the delivery itself, so the shape of the failure adds nothing here
            }

            return inputDelivery.Finished;
        }

        /// <summary>
        /// Gives the reader a moment to catch up and then takes whatever it has, finished or not.
        /// The reader fills a buffer owned out here rather than handing one back at the end, so the text is
        /// there either way and the wait only decides how much of it made it in. That is why the wait is
        /// capped and how it went is of no interest.
        /// Waiting on it for real would be waiting on a stream that may never come to an end - a cancellation token
        /// can't interrupt a read already in progress, so there would be nothing to break the wait.
        /// </summary>
        private static string HarvestErrorOutput(Task stdErrorTask, ErrorOutputBuffer errorOutput, int exitTimeoutMs)
        {
            if (errorOutput == null)
                return null;

            try
            {
                stdErrorTask?.Wait(exitTimeoutMs);
            }
            catch
            {
                // The reader gave up on something unforeseen. Whatever it collected before that still stands,
                // and is worth more here than the failure of the reading replacing the failure being reported.
            }

            return errorOutput.GetText();
        }
    }
}