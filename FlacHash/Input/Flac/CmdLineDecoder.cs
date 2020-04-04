using System;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.Input.Flac
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
            using (var process = GetProcess(sourceFile, decoderExecutableFile))
            {
                process.Start();

                var hashStream = new MemoryStream();
                process.StandardOutput.BaseStream.CopyTo(hashStream);

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
                        throw new Exception($"Somethn' went wron': {reader.ReadToEnd()}"); // TODO: exception type
                    }                    
                }

                return hashStream;
            }
        }

        private static Process GetProcess(FileInfo sourceFile, FileInfo decoderExecutableFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            var processSettings = GetProcessSettings(decoderExecutableFile);

            processSettings.ArgumentList.Add(DecoderOptions.Decode);
            processSettings.ArgumentList.Add(DecoderOptions.WriteToSdtOut);

            processSettings.ArgumentList.Add(sourceFile.FullName);

            return new Process
            {
                StartInfo = processSettings
            };
        }

        private static ProcessStartInfo GetProcessSettings(FileInfo encoderExecutablePath)
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