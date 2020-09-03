using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public interface ICompressedSizeService
    {
        long GetCompressedSize(FileInfo sourceFile, int compressionLevel);
    }

    public class CompressedSizeService : ICompressedSizeService
    {
        private readonly IO.Audio.IAudioFileEncoder encoder;

        public CompressedSizeService(IO.Audio.IAudioFileEncoder encoder)
        {
            this.encoder = encoder;
        }

        public long GetCompressedSize(FileInfo sourceFile, int compressionLevel)
        {
            using (Stream recodedAudio = encoder.Encode(sourceFile, compressionLevel))
            {
                return recodedAudio.Length;
            }
        }
    }
}