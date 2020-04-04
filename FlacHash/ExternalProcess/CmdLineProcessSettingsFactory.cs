using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.ExternalProcess
{
    public class CmdLineProcessSettingsFactory
    {
        public static ProcessStartInfo GetProcessSettings(FileInfo encoderExecutablePath)
        {
            if (encoderExecutablePath == null) throw new ArgumentNullException(nameof(encoderExecutablePath));

            return new ProcessStartInfo
            {
                FileName = encoderExecutablePath.FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false
            };
        }
    }
}