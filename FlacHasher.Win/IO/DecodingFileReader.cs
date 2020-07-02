using Andy.FlacHash.IO;
using Andy.FlacHash.IO.Audio;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win.IO
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

        public Stream Read(FileInfo sourceFile)
        {
            using (var stream = inputStreamFactory.CreateStream(sourceFile))
                return decoder.Read(stream);
        }
    }
}