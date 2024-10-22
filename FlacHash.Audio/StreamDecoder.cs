using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.Audio
{
    public class StreamDecoder : IAudioDecoder
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IIOProcessRunner processRunner;
        private readonly ICollection<string> @params;

        public StreamDecoder(
            FileInfo decoderExecutableFile,
            ExternalProcess.IIOProcessRunner processRunner,
            ICollection<string> @params)
        {
            this.decoderExecutableFile = decoderExecutableFile ?? throw new ArgumentNullException(nameof(decoderExecutableFile));
            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            this.@params = @params ?? throw new ArgumentNullException(nameof(@params));
        }

        public DecoderStream Read(Stream wavAudio, CancellationToken cancellation = default)
        {
            return new DecoderStream(processRunner.RunAndReadOutput(decoderExecutableFile, @params, wavAudio, cancellation));
        }
    }
}