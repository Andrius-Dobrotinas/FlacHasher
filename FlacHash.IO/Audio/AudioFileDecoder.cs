using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Andy.FlacHash.IO;

namespace Andy.FlacHash.Audio
{
    public class AudioFileDecoder : IAudioFileDecoder
    {
        private readonly IReadStreamFactory inputStreamFactory;
        private readonly IAudioDecoder decoder;

        public AudioFileDecoder(
            IReadStreamFactory streamFactory,
            IAudioDecoder decoder)
        {
            this.decoder = decoder;
            inputStreamFactory = streamFactory;
        }

        public Stream Read(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            // this gets disposed of by the decoder
            var stream = inputStreamFactory.CreateStream(sourceFile);
            return decoder.Read(stream, cancellation);
        }
    }
}