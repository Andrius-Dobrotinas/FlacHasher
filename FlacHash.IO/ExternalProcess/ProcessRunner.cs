using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.ExternalProcess
{
    public interface IIOProcessRunner
    {
        MemoryStream RunAndReadOutput(
            FileInfo fileToRun,
            IEnumerable<string> arguments,
            Stream input);
    }

    public interface IOutputOnlyProcessRunner
    {
        /// <summary>
        /// Runs a process and returns the contents of its output stream
        /// </summary>
        MemoryStream RunAndReadOutput(
            FileInfo fileToRun,
            IEnumerable<string> arguments);
    }

    public class ProcessRunner : IIOProcessRunner, IOutputOnlyProcessRunner
    {
        public MemoryStream RunAndReadOutput(
            FileInfo fileToRun,
            IEnumerable<string> arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(fileToRun);

            foreach (var arg in arguments)
                processSettings.ArgumentList.Add(arg);            

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
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            if (input == null) throw new ArgumentNullException(nameof(input));

            var processSettings = ProcessStartInfoFactory.GetStandardProcessSettings(fileToRun);
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

            return processOutput;
        }

        private static void ProcessExitCode(Process process)
        {
            if (!process.HasExited)
            {
                //should exit right away, this is just in case
                process.WaitForExit(1000);

                if (!process.HasExited)
                    process.Kill(true);
            }

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