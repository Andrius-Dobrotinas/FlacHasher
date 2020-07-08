using System;
using System.Collections.Generic;
using System.Text;

namespace Andy.FlacHash.IO.Audio.Flac
{
    public static class CompressionLevelValidation
    {
        public static void ValidateCompressionLevel(uint compressionLevel)
        {
            if (compressionLevel > (uint)CompressionLevel.Highest) throw new ArgumentOutOfRangeException(
                $"FLAC Compression level must be between {CompressionLevel.Lowest} and {CompressionLevel.Highest}");
        }
    }
}