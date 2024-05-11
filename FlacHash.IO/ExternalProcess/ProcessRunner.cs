using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Andy.FlacHash.ExternalProcess
{
    public class ProcessRunner : IIOProcessRunner, IOutputOnlyProcessRunner
    {
        private readonly int timeout;
        private readonly bool showProcessWindowWithStdErrOutput;

        /// <param name="timeout">Time to wait for the process to exit after all of its stdout has been read. Shouldn't be a large value because most processes exit right after finishing to write to stdout.</param>
        /// <param name="showProcessWindowWithStdErrOutput">When on, doesn't capture the process' stderror and therefore can't report errors - but the info is there for the user to see in window.
        /// When off, captures stderr and includes in exceptions if the process fails</param>
        public ProcessRunner(int timeout, bool showProcessWindowWithStdErrOutput)
        {
            this.timeout = timeout;
            this.showProcessWindowWithStdErrOutput = showProcessWindowWithStdErrOutput;
        }

        public Stream RunAndReadOutput(
            FileInfo fileToRun,
            IEnumerable<string> arguments)
        {
            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(fileToRun, arguments, showProcessWindowWithStdErrOutput);
            
            using (var process = new Process { StartInfo = processSettings })
            {
                process.Start();

                //Error (progress) stream has to be actively read as when the buffer fills up, the process stops writing to std-out.
                //The bigger the file, the more is written to the error stream as progress report
                var stdErrorTask = processSettings.RedirectStandardError
                    ? Task.Run<MemoryStream>(() => ReadStream(process.StandardError.BaseStream))
                    : null;

                var outputStream = ReadStream(process.StandardOutput.BaseStream);

                ProcessExitCode(process, stdErrorTask);

                return outputStream;
            }
        }

        public Stream RunAndReadOutput(
            FileInfo fileToRun,
            IEnumerable<string> arguments,
            Stream inputData)
        {
            if (inputData == null) throw new ArgumentNullException(nameof(inputData));

            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(fileToRun, arguments, showProcessWindowWithStdErrOutput);
            processSettings.RedirectStandardInput = true;

            using (var process = new Process { StartInfo = processSettings })
            {
                process.Start();

                Task.Delay(100).Wait(); //throws a "Pipe ended" error when trying to write to std right away. Waiting a bit before writing seems to solve the problem, but this could be problematic if the system is slower...

                //the writing operation gets blocked until someone reads the output - when both in and out are redirected.
                var outputReadTask = Task.Run<MemoryStream>(() => ReadStream(process.StandardOutput.BaseStream));

                //Error (progress) stream has to be actively read as when the buffer fills up, the process stops writing to std-out.
                //The bigger the file, the more is written to the error stream as progress report
                var stdErrorTask = processSettings.RedirectStandardError
                    ? Task.Run<MemoryStream>(() => ReadStream(process.StandardError.BaseStream))
                    : null;

                WriteToStdIn(process, inputData);

                var outputStream = GetTaskResult(outputReadTask);

                ProcessExitCode(process, stdErrorTask);

                return outputStream;
            }
        }

        private static void WriteToStdIn(Process process, Stream inputData)
        {
            var stdin = process.StandardInput.BaseStream;
            inputData.CopyTo(stdin);

            //it looks like either the stream has to be closed, or an "end of file" char (-1 in int language) must be written to the stream
            stdin.Close();
        }

        private static Stream GetTaskResult(Task<MemoryStream> outputReadTask)
        {
            try
            {
                return outputReadTask.Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        private static MemoryStream ReadStream(Stream outputStream)
        {
            var processOutput = new MemoryStream();
            outputStream.CopyTo(processOutput);

            processOutput.Seek(0, SeekOrigin.Begin);

            return processOutput;
        }

        private void ProcessExitCode(Process process, Task<MemoryStream> stdErrorTask)
        {
            if (stdErrorTask == null)
                ProcessExitCode(process);
            else
            using (var stdError = stdErrorTask.GetAwaiter().GetResult())
            {
                ProcessExitCode(process, stdError);
            }
        }

        private void ProcessExitCode(Process process, Stream stdError = null)
        {
            //stop and wait for the process to finish
            if (process.WaitForExit(timeout) == false)
                process.Kill(true);

            if (process.ExitCode != 0)
            {
                if (stdError == null)
                    throw new ExecutionException(process.ExitCode);

                string processErrorOutput = GetErrorStreamOutput(stdError);
                throw new ExecutionException(process.ExitCode, processErrorOutput);
            }
        }

        private static string GetErrorStreamOutput(Stream stdError)
        {
            using (var reader = new StreamReader(stdError))
            {
                try
                {
                    return reader.ReadToEnd();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}