using System;
using System.Collections.Generic;
using System.IO;
using Andy.FlacHash.Audio.Flac;
using Andy.FlacHash.Audio.Flac.CmdLine;

namespace Andy.FlacHash.Audio
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
            CompressionLevelValidation.ValidateCompressionLevel(compressionLevel);

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
                Parameters.Options.Encoder.Stdout //if that doesn't work, "-" could
            };
        }
    }
}