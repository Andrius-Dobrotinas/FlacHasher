using System;
using System.Diagnostics;
using System.IO;

namespace FlacHasher.Input.Flac
{
    // TODO: Flac.exe can also take input via stdin. See if I want to go that way.

    public class CmdLineDecoder : IFileReader
    {
        private readonly FileInfo decoderExecutableFile;

        public CmdLineDecoder(FileInfo decoderExecutableFile)
        {
            if (decoderExecutableFile == null) throw new ArgumentNullException(nameof(decoderExecutableFile));

            this.decoderExecutableFile = decoderExecutableFile;
        }

        public Stream Read(FileInfo sourceFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            var processSettings = GetProcessSettings(decoderExecutableFile);

            processSettings.ArgumentList.Add(DecoderOptions.Decode);
            processSettings.ArgumentList.Add(DecoderOptions.WriteToSdtOut);

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