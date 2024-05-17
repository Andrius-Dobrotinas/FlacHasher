using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.IO.Audio.Flac.CmdLine
{
    public class StreamDecoder : IAudioDecoder
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IIOProcessRunner processRunner;

        public StreamDecoder(FileInfo decoderExecutableFile,
            ExternalProcess.IIOProcessRunner processRunner)
        {
            this.decoderExecutableFile = decoderExecutableFile ?? throw new ArgumentNullException(nameof(decoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public Stream Read(Stream wavAudio, CancellationToken cancellation = default)
        {
            var arguments = GetProcessArguments();

            try
            {
                return processRunner.RunAndReadOutput(decoderExecutableFile, arguments, wavAudio, cancellation);
            }
            catch (ExternalProcess.ExecutionException e)
            {
                throw new FlacCompressionException("Failed to read the input file", e);
            }
        }

        private static string[] GetProcessArguments()
        {
            return new string[]
            {
                DecoderOptions.Decode,
                DecoderOptions.ReadFromStdIn,
            };
        }
    }
}