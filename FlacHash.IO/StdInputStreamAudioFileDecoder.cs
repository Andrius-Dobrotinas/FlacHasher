using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.Audio
{
    /// <summary>
    /// Feeds file data stream to the decoder via std-in
    /// </summary>
    public class StdInputStreamAudioFileDecoder : IAudioFileDecoder
    {
        private readonly IFileReadStreamFactory inputStreamFactory;
        private readonly IAudioDecoder decoder;

        public StdInputStreamAudioFileDecoder(
            IFileReadStreamFactory streamFactory,
            IAudioDecoder decoder)
        {
            this.decoder = decoder;
            inputStreamFactory = streamFactory;
        }

        public DecoderStream Read(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            // this gets disposed of by the decoder
            var stream = inputStreamFactory.CreateStream(sourceFile);
            return decoder.Read(stream, cancellation);
        }
    }
}