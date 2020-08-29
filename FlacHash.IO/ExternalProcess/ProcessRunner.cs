using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.ExternalProcess
{
    public class ProcessRunner : IIOProcessRunner, IOutputOnlyProcessRunner
    {
        //todo: inject this value
        private readonly int timeout;

        public ProcessRunner(int timeout)
        {
            this.timeout = timeout;
        }

        public MemoryStream RunAndReadOutput(
            FileInfo fileToRun,
            IEnumerable<string> arguments)
        {
            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(fileToRun, arguments);

            using (var process = new Process { StartInfo = processSettings })
            {
                process.Start();

                var output = GetStandardOutput(process);

                ProcessExitCode(process);

                return output;
            }
        }

        public MemoryStream RunAndReadOutput(
            FileInfo fileToRun,
            IEnumerable<string> arguments,
            Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(fileToRun, arguments);
            processSettings.RedirectStandardInput = true;

            using (var process = new Process { StartInfo = processSettings })
            {
                process.Start();

                System.Threading.Tasks.Task.Delay(100).Wait(); //throws a "Pipe ended" error when trying to write to std right away. Waiting a bit before writing seems to solve the problem, but this could be problematic if the system is slower...

                //the writing operation gets blocked until someone reads the output - when both in and out are redirected... at least with Flac
                var outputTask = System.Threading.Tasks.Task.Run<MemoryStream>(() => GetStandardOutput(process));

                var stdin = process.StandardInput.BaseStream;
                input.CopyTo(stdin);

                //it looks like either the stream has to be closed, or an "end of file" char (-1 in int language) must be written to the stream
                stdin.Close();

                ProcessExitCode(process);

                try
                {
                    return outputTask.Result;
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            }
        }

        private static MemoryStream GetStandardOutput(Process process)
        {
            var processOutput = new MemoryStream();
            process.StandardOutput.BaseStream.CopyTo(processOutput);

            processOutput.Seek(0, SeekOrigin.Begin);

            return processOutput;
        }

        private void ProcessExitCode(Process process)
        {
            //have to stop and wait for the process to finish
            if (process.WaitForExit(timeout) == false)
                process.Kill(true);

            if (process.ExitCode != 0)
            {
                string processErrorOutput = GetErrorStreamOutput(process);
                throw new ExecutionException(process.ExitCode, processErrorOutput);
            }
        }

        private static string GetErrorStreamOutput(Process process)
        {
            using (var reader = new StreamReader(process.StandardError.BaseStream))
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