using Andy.FlacHash.ExternalProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac
{
    public class CmdLineFlacRecoder : IAudioFileEncoder
    {
        private class EncoderFlags
        {
            public static string Stdout = "--stdout";
        }

        private readonly FileInfo decoderExecutableFile;
        private readonly IOutputOnlyProcessRunner processRunner;

        public CmdLineFlacRecoder(FileInfo encoderExecutableFile,
            IOutputOnlyProcessRunner processRunner)
        {
            this.decoderExecutableFile = encoderExecutableFile ?? throw new ArgumentNullException(nameof(encoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public MemoryStream Encode(FileInfo sourceFile, uint compressionLevel)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            // todo: take the min/max values from a single place
            if (compressionLevel > 8) throw new ArgumentOutOfRangeException(
                "FLAC Compression level must be between 0 and 8");

            var arguments = GetProcessArguments(compressionLevel, sourceFile);

            try
            {
                return processRunner.RunAndReadOutput(
                    decoderExecutableFile,
                    arguments);
            }
            catch (ExecutionException e)
            {
                var message = $"Failed to re-encode the file. FLAC process exited with error code {e.ExitCode}.\nFLAC error output: {e.ProcessErrorOutput}";

                throw new AudioCompressionException(message);
            }
        }

        private static string[] GetProcessArguments(
            uint compressionLevel,
            FileInfo sourceFile)
        {
            return new string[]
            {
                $"-{compressionLevel}",
                EncoderFlags.Stdout,
                sourceFile.FullName
            };
        }
    }
}