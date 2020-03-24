using System;
using System.Diagnostics;
using System.IO;

namespace FlacHasher
{
    public class AudioDecoder : IAudioDecoder
    {
        private readonly string encoderExecutablePath;

        public AudioDecoder(string encoderExecutablePath)
        {
            if (string.IsNullOrEmpty(encoderExecutablePath)) throw new ArgumentNullException(nameof(encoderExecutablePath), "The value is either null or empty");

            this.encoderExecutablePath = encoderExecutablePath;
        }

        public Stream Decode(FileInfo sourceFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            var processSettings = GetProcessSettings(encoderExecutablePath);

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

        private static ProcessStartInfo GetProcessSettings(string encoderExecutablePath)
        {
            return new ProcessStartInfo
            {
                FileName = encoderExecutablePath,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false
            };
        }
    }
}