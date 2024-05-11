using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.ExternalProcess
{
    internal static class ProcessStartInfoFactory
    {
        internal static ProcessStartInfo GetStandardProcessSettings(
            FileInfo fileToRun,
            IEnumerable<string> arguments,
            bool showProcessWindowWithStdErrOutput)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            var settings = GetStandardProcessSettings(fileToRun, showProcessWindowWithStdErrOutput);

            foreach (var arg in arguments)
                settings.ArgumentList.Add(arg);

            return settings;
        }

        internal static ProcessStartInfo GetStandardProcessSettings(FileInfo fileToRun, bool showProcessWindowWithStdErrOutput)
        {
            if (fileToRun == null) throw new ArgumentNullException(nameof(fileToRun));

            return new ProcessStartInfo
            {
                FileName = fileToRun.FullName,
                RedirectStandardError = showProcessWindowWithStdErrOutput ? false : true,
                RedirectStandardOutput = true,
                UseShellExecute = false, // Required for stream redirection to work
                CreateNoWindow = showProcessWindowWithStdErrOutput ? false : true,
                ErrorDialog = false
            };
        }
    }
}