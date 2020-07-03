using Andy.FlacHash.ExternalProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac
{
    public class CmdLineFlacEncoder : IAudioEncoder
    {
        private class EncoderFlags
        {
            public static string Stdout = "-";
        }

        private readonly FileInfo decoderExecutableFile;
        private readonly IIOProcessRunner processRunner;

        public CmdLineFlacEncoder(FileInfo encoderExecutableFile,
            IIOProcessRunner processRunner)
        {
            this.decoderExecutableFile = encoderExecutableFile ?? throw new ArgumentNullException(nameof(encoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public MemoryStream Encode(Stream wavAudio, uint compressionLevel)
        {
            var processSettings = GetProcessSettings(decoderExecutableFile, compressionLevel);

            try
            {
                return processRunner.RunAndReadOutput(processSettings, wavAudio);
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