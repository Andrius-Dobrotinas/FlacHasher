using Andy.FlacHash.ExternalProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.Audio.Compression
{
    public class CmdLineFlacEncoder : IAudioEncoder
    {
        private class EncoderFlags
        {
            public static string Stdout = "-";
        }

        private readonly FileInfo decoderExecutableFile;
        private readonly uint compressionLevel;

        public CmdLineFlacEncoder(FileInfo encoderExecutableFile, uint compressionLevel)
        {
            this.decoderExecutableFile = encoderExecutableFile ?? throw new ArgumentNullException(nameof(encoderExecutableFile));

            if (compressionLevel > 8) throw new ArgumentOutOfRangeException(
                "FLAC Compression level must be between 0 and 8");

            this.compressionLevel = compressionLevel;
        }

        public MemoryStream Encode(Stream wavAudio)
        {
            var processSettings = GetProcessSettings(decoderExecutableFile, compressionLevel);

            try
            {
                return ProcessRunner.RunAndReadOutput(processSettings, wavAudio);
            }
            catch (ExecutionException e)
            {
                var message = $"Failed to encode the file. FLAC process exited with error code {e.ExitCode}.\nFLAC error output: {e.ProcessErrorOutput}";

                throw new AudioCompressionException(message);
            }
        }

        private static ProcessStartInfo GetProcessSettings(FileInfo encoderExecutableFile, uint compressionLevel)
        {
            var processSettings = CmdLineProcessSettingsFactory.GetProcessSettings(encoderExecutableFile);

            processSettings.ArgumentList.Add($"-{compressionLevel}");
            processSettings.ArgumentList.Add(EncoderFlags.Stdout);

            return processSettings;
        }
    }
}