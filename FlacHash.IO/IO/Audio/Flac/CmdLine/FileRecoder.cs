using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac.CmdLine
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

        public MemoryStream Encode(FileInfo sourceFile, uint compressionLevel)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            CompressionLevelValidation.ValidateCompressionLevel(compressionLevel);

            var arguments = GetProcessArguments(compressionLevel, sourceFile);

            try
            {
                return processRunner.RunAndReadOutput(
                    recoderExecutableFile,
                    arguments);
            }
            catch (ExternalProcess.ExecutionException e)
            {
                throw new FlacCompressionException("Failed to re-encode the file", e);
            }
        }

        private static string[] GetProcessArguments(
            uint compressionLevel,
            FileInfo sourceFile)
        {
            return new string[]
            {
                $"-{compressionLevel}",
                EncoderOptions.Stdout,
                sourceFile.FullName
            };
        }
    }
}