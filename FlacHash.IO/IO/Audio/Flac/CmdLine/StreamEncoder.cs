﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac.CmdLine
{
    public class StreamEncoder : IAudioEncoder
    {
        private class EncoderFlags
        {
            public static string Stdout = "-";
        }

        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IIOProcessRunner processRunner;

        public StreamEncoder(FileInfo encoderExecutableFile,
            ExternalProcess.IIOProcessRunner processRunner)
        {
            this.decoderExecutableFile = encoderExecutableFile ?? throw new ArgumentNullException(nameof(encoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public MemoryStream Encode(Stream wavAudio, uint compressionLevel)
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