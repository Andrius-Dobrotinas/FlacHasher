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
        public static Stream RunAndReadOutput(ProcessStartInfo processSettings)
        {
            // TODO: possibly create a new type for settings for this method. It's better for this method to redirect the streams

            if (processSettings.RedirectStandardOutput == false) throw new ArgumentException($"For this to work, process's standard output must be redirected ({nameof(ProcessStartInfo.RedirectStandardOutput)} property)");

            if (processSettings.RedirectStandardError == false) throw new ArgumentException($"For this to work, process's standard output must be redirected ({nameof(ProcessStartInfo.RedirectStandardError)} property)");

            using (var process = new Process { StartInfo = processSettings })
            {
                process.Start();

                var processOutput = new MemoryStream();
                process.StandardOutput.BaseStream.CopyTo(processOutput);

                if (!process.HasExited)
                {
                    // Should exit right away, this is just in case
                    process.WaitForExit(1000);
                    process.Kill(true);
                }

                if (process.ExitCode != 0)
                {
                    using (var reader = new StreamReader(process.StandardError.BaseStream))
                    {
                        string processErrorOutput;
                        try
                        {
                            processErrorOutput = reader.ReadToEnd();
                        }
                        catch (Exception)
                        {
                            processErrorOutput = null;
                        }
                        throw new ExecutionException(process.ExitCode, processErrorOutput); // TODO: exception type
                    }
                }

                return processOutput;
            }
        }
    }
}