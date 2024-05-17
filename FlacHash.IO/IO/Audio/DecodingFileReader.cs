using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.IO.Audio
{
    public class DecodingFileReader : IFileReader
    {
        private readonly IInputStreamFactory inputStreamFactory;
        private readonly IAudioDecoder decoder;

        public DecodingFileReader(
            IInputStreamFactory streamFactory,
            IAudioDecoder decoder)
        {
            this.decoder = decoder;
            this.inputStreamFactory = streamFactory;
        }

        public Stream Read(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            using (var stream = inputStreamFactory.CreateStream(sourceFile))
                return decoder.Read(stream, cancellation);
        }
    }
}