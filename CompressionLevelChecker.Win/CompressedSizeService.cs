using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public interface ICompressedSizeService
    {
        long GetCompressedSize(FileInfo sourceFile, uint compressionLevel);
    }

    public class CompressedSizeService : ICompressedSizeService
    {
        private readonly Audio.Compression.ICmdLineFlacRecoder encoder;

        public CompressedSizeService(Audio.Compression.ICmdLineFlacRecoder encoder)
        {
            this.encoder = encoder;
        }

        public long GetCompressedSize(FileInfo sourceFile, uint compressionLevel)
        {
            using (MemoryStream recodedAudio = encoder.Encode(sourceFile, compressionLevel))
            {
                return recodedAudio.Length;
            }
        }
    }
}