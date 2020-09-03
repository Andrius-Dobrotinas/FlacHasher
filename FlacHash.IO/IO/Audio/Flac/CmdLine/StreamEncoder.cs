using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac.CmdLine
{
    public class StreamEncoder : IAudioEncoder
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IIOProcessRunner processRunner;

        public StreamEncoder(FileInfo encoderExecutableFile,
            ExternalProcess.IIOProcessRunner processRunner)
        {
            this.decoderExecutableFile = encoderExecutableFile ?? throw new ArgumentNullException(nameof(encoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public Stream Encode(Stream wavAudio, int compressionLevel)
        {
            if (wavAudio == null) throw new ArgumentNullException(nameof(wavAudio));
            CompressionLevelValidation.ValidateCompressionLevel(compressionLevel);

            var arguments = GetProcessArguments(compressionLevel);

            try
            {
                return processRunner.RunAndReadOutput(
                    decoderExecutableFile,
                    arguments,
                    wavAudio);
            }
            catch (ExternalProcess.ExecutionException e)
            {
                throw new FlacCompressionException("Failed to encode the file", e);
            }
        }

        private static string[] GetProcessArguments(int compressionLevel)
        {
            return new string[]
            {
                $"-{compressionLevel}",
                EncoderOptions.Stdout //if that doesn't work, "-" could
            };
        }
    }
}