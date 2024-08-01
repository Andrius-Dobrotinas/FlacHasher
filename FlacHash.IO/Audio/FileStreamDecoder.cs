using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Andy.FlacHash.IO;

namespace Andy.FlacHash.Audio
{
    public class FileStreamDecoder : IAudioFileDecoder
    {
        private readonly IInputStreamFactory inputStreamFactory;
        private readonly IAudioDecoder decoder;

        public FileStreamDecoder(
            IInputStreamFactory streamFactory,
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