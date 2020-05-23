using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public class CompressedSizeService
    {
        private readonly CmdLineFlacEncoderFactory recoderFactory;

        public CompressedSizeService(CmdLineFlacEncoderFactory recoderFactory)
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