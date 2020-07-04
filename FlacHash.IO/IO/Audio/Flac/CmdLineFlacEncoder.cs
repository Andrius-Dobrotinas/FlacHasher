using Andy.FlacHash.ExternalProcess;
using System;
using System.Collections.Generic;
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
            if (wavAudio == null) throw new ArgumentNullException(nameof(wavAudio));

            // todo: take the min/max values from a single place
            if (compressionLevel > 8) throw new ArgumentOutOfRangeException(
                "FLAC Compression level must be between 0 and 8");

            var arguments = GetProcessArguments(compressionLevel);

            try
            {
                return processRunner.RunAndReadOutput(
                    decoderExecutableFile,
                    arguments,
                    wavAudio);
            }
            catch (ExecutionException e)
            {
                var message = $"Failed to encode the file. FLAC process exited with error code {e.ExitCode}.\nFLAC error output: {e.ProcessErrorOutput}";

                throw new AudioCompressionException(message);
            }
        }

        private static string[] GetProcessArguments(uint compressionLevel)
        {
            return new string[]
            {
                $"-{compressionLevel}",
                EncoderFlags.Stdout
            };
        }
    }
}