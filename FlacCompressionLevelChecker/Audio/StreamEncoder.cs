using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.CompressionLevel.Audio
{
    public class StreamEncoder : IAudioEncoder
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IIOProcessRunner processRunner;

        public StreamEncoder(FileInfo encoderExecutableFile,
            ExternalProcess.IIOProcessRunner processRunner)
        {
            decoderExecutableFile = encoderExecutableFile ?? throw new ArgumentNullException(nameof(encoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public Stream Encode(Stream wavAudio, int compressionLevel)
        {
            if (wavAudio == null) throw new ArgumentNullException(nameof(wavAudio));
            Andy.FlacHash.Audio.Flac.CompressionLevelValidation.ValidateCompressionLevel(compressionLevel);

            var arguments = GetProcessArguments(compressionLevel);

            return processRunner.RunAndReadOutput(
                decoderExecutableFile,
                arguments,
                wavAudio);
        }

        private static string[] GetProcessArguments(int compressionLevel)
        {
            return new string[]
            {
                $"-{compressionLevel}",
                FlacHash.Audio.Flac.Parameters.Options.Encoder.Stdout //if that doesn't work, "-" could
            };
        }
    }
}