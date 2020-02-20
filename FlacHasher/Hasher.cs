using FlacHasher.Crypto;
using System;
using System.Diagnostics;

namespace FlacHasher
{
    public class Hasher
    {
        private readonly string encoderExecutablePath;
        private readonly IHashComputer hashComputer;

        public Hasher(string encoderExecutablePath, IHashComputer hashComputer)
        {
            if (string.IsNullOrEmpty(encoderExecutablePath)) throw new ArgumentNullException(nameof(encoderExecutablePath), "The value is either null or empty");

            if (hashComputer == null) throw new ArgumentNullException(nameof(hashComputer));

            this.encoderExecutablePath = encoderExecutablePath;
            this.hashComputer = hashComputer;
        }

        public byte[] ComputerHash(string sourceFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath)) throw new ArgumentNullException(nameof(sourceFilePath), "The value is either null or empty");

            var processSettings = new ProcessStartInfo
            {
                FileName = encoderExecutablePath,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false
            };

            processSettings.ArgumentList.Add(FlacOptions.Decode);
            processSettings.ArgumentList.Add(FlacOptions.WriteToSdtOut);
            processSettings.ArgumentList.Add(sourceFilePath);

            var encoder = new Process
            {
                StartInfo = processSettings
            };

            encoder.Start();

            return hashComputer.ComputeHash(encoder.StandardOutput.BaseStream);
        }
    }
}