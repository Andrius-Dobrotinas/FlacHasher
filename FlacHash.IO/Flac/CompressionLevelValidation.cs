using System;
using System.Collections.Generic;
using System.Text;

namespace Andy.FlacHash.Audio.Flac
{
    public static class CompressionLevelValidation
    {
        public static void ValidateCompressionLevel(int compressionLevel)
        {
            if (compressionLevel > (int)CompressionLevel.Highest) throw new ArgumentOutOfRangeException(
                $"FLAC Compression level must be between {CompressionLevel.Lowest} and {CompressionLevel.Highest}");
        }
    }
}