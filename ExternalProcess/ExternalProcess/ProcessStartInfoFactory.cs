using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.ExternalProcess
{
    internal static class ProcessStartInfoFactory
    {
        internal static ProcessStartInfo GetStandardProcessSettings(
            FileInfo fileToRun,
            IEnumerable<string> arguments,
            bool showProcessOutput)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            var settings = GetStandardProcessSettings(fileToRun, showProcessOutput);

            foreach (var arg in arguments)
                settings.ArgumentList.Add(arg);

            return settings;
        }

        internal static ProcessStartInfo GetStandardProcessSettings(FileInfo fileToRun, bool showProcessWindowWithStdErrOutput)
        {
            if (fileToRun == null) throw new ArgumentNullException(nameof(fileToRun));

            /* In order to write process' output directly to my console, I need CreateNoWindow: false & RedirectStandardError: true
             * 
             * Process stderr output capturing combinations:
             * RedirectStandardError:false | CreateNoWindow:false -> Writes to my console + I don't capture stderr directly
             * RedirectStandardError | CreateNoWindow:false -> No write to my console
             * RedirectStandardError | CreateNoWindow -> No write to my console
             * RedirectStandardError:false | CreateNoWindow -> No write to my console + I don't capture stderr directly
             * 
             * CreateNoWindow:true -> means the process never gets console, I will never see the output, even if I redirect the stderr
             * CreateNoWindow:false -> makes the process get a window - my window!
             * 
             * So I want CreateNoWindow to always be False. Then I just either:
             * - redirect stderr to hide the output from the console (I want to capture that output for error scenarios)
             * - no-redirect-stderr to show output in the console
             * 
             * Man, this is so confusing. Why couldn't they at least call it CreateWindow? Thinking about negating the negative is hard.
             */
            return new ProcessStartInfo
            {
                FileName = fileToRun.FullName,
                RedirectStandardError = showProcessWindowWithStdErrOutput ? false : true,
                RedirectStandardOutput = true,
                UseShellExecute = false, // Required for stream redirection to work. With Shell execution, it launches a new process (with or without a window, depending on CreateNoWindow), which means it won't write to my console and won't support redirecting streams
                CreateNoWindow = false,
                ErrorDialog = false // only applies when doing Shell-execute
            };
        }
    }
}