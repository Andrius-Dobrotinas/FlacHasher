using System;
using System.Diagnostics;
using System.IO;

namespace FlacHasher
{
    public class FlacDecoder : IAudioDecoder
    {
        private readonly FileInfo encoderExecutableFile;

        public FlacDecoder(FileInfo encoderExecutablePath)
        {
            if (encoderExecutablePath == null) throw new ArgumentNullException(nameof(encoderExecutablePath));

            this.encoderExecutableFile = encoderExecutablePath;
        }

        public Stream Decode(FileInfo sourceFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            var processSettings = GetProcessSettings(encoderExecutableFile);

            processSettings.ArgumentList.Add(FlacOptions.Decode);
            processSettings.ArgumentList.Add(FlacOptions.WriteToSdtOut);

            processSettings.ArgumentList.Add(sourceFile.FullName);

            var process = new Process
            {
                StartInfo = processSettings
            };

            process.Start();

            return process.StandardOutput.BaseStream;
        }

        private static ProcessStartInfo GetProcessSettings(FileInfo encoderExecutablePath)
        {
            return new ProcessStartInfo
            {
                FileName = encoderExecutablePath.FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false
            };
        }
    }
}