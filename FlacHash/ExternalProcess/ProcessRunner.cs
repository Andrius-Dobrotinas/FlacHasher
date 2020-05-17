using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.ExternalProcess
{
    public class ProcessRunner
    {
        /// <summary>
        /// Runs an process and returns the contents of its output stream
        /// </summary>
        public static MemoryStream RunAndReadOutput(ProcessStartInfo processSettings)
        {
            // TODO: possibly create a new type for settings for this method. It's better for this method to redirect the streams

            if (processSettings.RedirectStandardOutput == false) throw new ArgumentException($"For this to work, process's standard output must be redirected ({nameof(ProcessStartInfo.RedirectStandardOutput)} property)");

            if (processSettings.RedirectStandardError == false) throw new ArgumentException($"For this to work, process's standard output must be redirected ({nameof(ProcessStartInfo.RedirectStandardError)} property)");

            using (var process = new Process { StartInfo = processSettings })
            {
                process.Start();

                var output = GetStandardOutput(process);

                ProcessExitCode(process);

                return output;
            }
        }

        public static MemoryStream RunAndReadOutput(ProcessStartInfo processSettings, Stream input)
        {
            processSettings.RedirectStandardOutput = true;
            processSettings.RedirectStandardError = true;
            processSettings.RedirectStandardInput = true;

            using (var process = new Process { StartInfo = processSettings })
            {
                process.Start();

                System.Threading.Tasks.Task.Delay(100).Wait(); //throws a "Pipe ended" error when trying to write to std right away. Waiting a bit before writing seems to solve the problem, but this could be problematic if the system is slower...

                //the writing operation gets blocked until someone reads the output - when both in and out are redirected... at least with Flac
                var outputTask = System.Threading.Tasks.Task.Run<MemoryStream>(() => GetStandardOutput(process));

                input.CopyTo(process.StandardInput.BaseStream);

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