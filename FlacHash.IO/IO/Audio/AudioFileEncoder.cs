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

        public MemoryStream Encode(FileInfo sourceFile, int compressionLevel)
        {
            using (Stream rawAudio = fileReader.Read(sourceFile))
            {

                // TODO
                rawAudio.Seek(0, SeekOrigin.Begin);

                return encoder.Encode(rawAudio, compressionLevel);
            }
        }
    }
}