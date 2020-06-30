using Andy.FlacHash.Input;
using Andy.FlacHash.Input.Flac;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win.IO
{
    public class DecodingFileReader : IFileReader
    {
        private readonly IReadStreamFactory streamFactory;
        private readonly CmdLineAudioStreamDecoder decoder;

        public DecodingFileReader(
            IReadStreamFactory streamFactory,
            CmdLineAudioStreamDecoder decoder)
        {
            this.decoder = decoder;
            this.streamFactory = streamFactory;
        }

        public Stream Read(FileInfo sourceFile)
        {
            using (var stream = streamFactory.CreateStream(sourceFile))
                return decoder.Read(stream);
        }
    }
}