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
        private readonly CmdLineStreamDecoder decoder;

        public DecodingFileReader(
            IReadStreamFactory streamFactory,
            CmdLineStreamDecoder decoder)
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