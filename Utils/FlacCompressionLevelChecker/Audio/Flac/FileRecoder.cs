using System;
using System.Collections.Generic;
using System.IO;
using Andy.FlacHash.Application.Audio.Flac;
using Andy.FlacHash.CompressionLevel.Audio;

namespace Andy.FlacHash.Audio.Flac
{
    public class FileRecoder : IAudioFileEncoder
    {
        private readonly FileInfo recoderExecutableFile;
        private readonly ExternalProcess.IOutputOnlyProcessRunner processRunner;

        public FileRecoder(FileInfo recoderExecutableFile,
            ExternalProcess.IOutputOnlyProcessRunner processRunner)
        {
            this.recoderExecutableFile = recoderExecutableFile ?? throw new ArgumentNullException(nameof(recoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public Stream Encode(FileInfo sourceFile, int compressionLevel)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            CompressionLevelValidation.ValidateCompressionLevel(compressionLevel);

            var arguments = GetProcessArguments(compressionLevel, sourceFile);

            return processRunner.RunAndReadOutput(
                recoderExecutableFile,
                arguments);
        }

        private static string[] GetProcessArguments(
            int compressionLevel,
            FileInfo sourceFile)
        {
            return new string[]
            {
                $"-{compressionLevel}",
                Parameters.Options.Encoder.Stdout,
                sourceFile.FullName
            };
        }
    }
}