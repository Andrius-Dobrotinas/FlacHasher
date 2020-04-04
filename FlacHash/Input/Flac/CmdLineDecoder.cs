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
            var processSettings = GetProcessSettings(sourceFile, decoderExecutableFile);

            return ExternalProcess.ProcessRunner.RunAndReadOutput(processSettings);
        }

        private static ProcessStartInfo GetProcessSettings(FileInfo sourceFile, FileInfo decoderExecutableFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            var processSettings = ExternalProcess.CmdLineProcessSettingsFactory.GetProcessSettings(decoderExecutableFile);

            processSettings.ArgumentList.Add(DecoderOptions.Decode);
            processSettings.ArgumentList.Add(DecoderOptions.WriteToSdtOut);

            processSettings.ArgumentList.Add(sourceFile.FullName);

            return processSettings;
        }
    }
}