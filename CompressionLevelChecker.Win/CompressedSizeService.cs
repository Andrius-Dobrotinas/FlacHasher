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
        private readonly ICmdLineFlacEncoderFactory recoderFactory;

        public CompressedSizeService(ICmdLineFlacEncoderFactory recoderFactory)
        {
            this.recoderFactory = recoderFactory;
        }

        public long GetCompressedSize(FileInfo sourceFile, uint compressionLevel)
        {
            var recoder = recoderFactory.Build(compressionLevel);

            using (MemoryStream recodedAudio = recoder.Encode(sourceFile))
            {
                return recodedAudio.Length;
            }
        }
    }
}