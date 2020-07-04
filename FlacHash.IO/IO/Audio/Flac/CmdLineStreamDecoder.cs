﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac
{
    public class CmdLineStreamDecoder : IAudioDecoder
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IIOProcessRunner processRunner;

        public CmdLineStreamDecoder(FileInfo decoderExecutableFile,
            ExternalProcess.IIOProcessRunner processRunner)
        {
            this.decoderExecutableFile = decoderExecutableFile ?? throw new ArgumentNullException(nameof(decoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public Stream Read(Stream wavAudio)
        {
            var arguments = GetProcessArguments();

            try
            {
                return processRunner.RunAndReadOutput(decoderExecutableFile, arguments, wavAudio);
            }
            catch (ExternalProcess.ExecutionException e)
            {
                throw new CmdLineCompressionException("Failed to read the input file", e);
            }
        }

        private static string[] GetProcessArguments()
        {
            return new string[]
            {
                CmdLineDecoderOptions.Decode,
                "-" // read from stdin
            };
        }
    }
}