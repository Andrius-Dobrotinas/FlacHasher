using System;
using System.IO;

namespace Andy.FlacHash.Audio
{
    public class AudioFileEncoder : IAudioFileEncoder
    {
        private readonly IAudioFileDecoder fileDecoder;
        private readonly IAudioEncoder encoder;

        public AudioFileEncoder(IAudioFileDecoder fileDecoder, IAudioEncoder encoder)
        {
            this.fileDecoder = fileDecoder;
            this.encoder = encoder;
        }

        public Stream Encode(FileInfo sourceFile, int compressionLevel)
        {
            // this gets disposed of by the encoder
            Stream rawAudio = fileDecoder.Read(sourceFile);
            return encoder.Encode(rawAudio, compressionLevel);
        }
    }
}