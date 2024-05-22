using Andy.FlacHash.IO;
using System;
using System.IO;

namespace Andy.FlacHash.IO.Audio
{
    public class AudioFileEncoder : IAudioFileEncoder
    {
        private readonly IFileDecoder fileDecoder;
        private readonly IAudioEncoder encoder;

        public AudioFileEncoder(IFileDecoder fileDecoder, IAudioEncoder encoder)
        {
            this.fileDecoder = fileDecoder;
            this.encoder = encoder;
        }

        public Stream Encode(FileInfo sourceFile, int compressionLevel)
        {
            // this gets disposed of by the decoder
            Stream rawAudio = fileDecoder.Read(sourceFile);
            return encoder.Encode(rawAudio, compressionLevel);
        }
    }
}