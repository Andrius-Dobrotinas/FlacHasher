using Andy.FlacHash.Input;
using System;
using System.IO;

namespace Andy.FlacHash.Audio.Compression.File
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

        public MemoryStream Encode(FileInfo sourceFile, uint compressionLevel)
        {
            using (Stream rawAudio = fileReader.Read(sourceFile))
            {
                rawAudio.Seek(0, SeekOrigin.Begin);

                return encoder.Encode(rawAudio, compressionLevel);
            }
        }
    }
}