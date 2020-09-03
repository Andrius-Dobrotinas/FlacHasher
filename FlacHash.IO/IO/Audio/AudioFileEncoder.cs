using Andy.FlacHash.IO;
using System;
using System.IO;

namespace Andy.FlacHash.IO.Audio
{
    public class AudioFileEncoder : IAudioFileEncoder
    {
        private readonly IFileReader fileReader;
        private readonly IAudioEncoder encoder;

        public AudioFileEncoder(IFileReader fileReader, IAudioEncoder encoder)
        {
            this.fileReader = fileReader;
            this.encoder = encoder;
        }

        public Stream Encode(FileInfo sourceFile, int compressionLevel)
        {
            using (Stream rawAudio = fileReader.Read(sourceFile))
            {
                return encoder.Encode(rawAudio, compressionLevel);
            }
        }
    }
}