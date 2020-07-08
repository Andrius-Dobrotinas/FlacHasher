﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac.CmdLine
{
    public class FileRecoder : IAudioFileEncoder
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IOutputOnlyProcessRunner processRunner;

        public FileRecoder(FileInfo encoderExecutableFile,
            ExternalProcess.IOutputOnlyProcessRunner processRunner)
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