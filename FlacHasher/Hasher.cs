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
            this.encoderExecutablePath = encoderExecutablePath;
            this.hashComputer = hashComputer;
        }

        public byte[] ComputerHash(string sourceFilePath)
        {
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