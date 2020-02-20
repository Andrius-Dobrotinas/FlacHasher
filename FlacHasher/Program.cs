﻿using FlacHasher.Crypto;
using System;
using System.Diagnostics;
using System.IO;

namespace FlacHasher
{
    class Program
    {
        static void Main(string[] args)
        {
            var encoderExecutablePath = @"C:\Program Files (x86)\FLAC Frontend\tools\flac.exe";
            var filePath = @"c:\01 - Hex Me.flac";

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
            processSettings.ArgumentList.Add(filePath);

            var encoder = new Process
            {
                StartInfo = processSettings
            };

            encoder.Start();

            var hashComputer = new Sha256HashComputer();

            byte[] hash;

            using (var waveData = new MemoryStream())
            {
                encoder.StandardOutput.BaseStream.CopyTo(waveData);

                hash = hashComputer.ComputeHash(waveData);
            }

            Console.WriteLine(BitConverter.ToString(hash));
        }
    }
}
