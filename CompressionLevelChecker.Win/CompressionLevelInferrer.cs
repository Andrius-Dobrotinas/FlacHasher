﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public class CompressionLevelInferrer
    {
        private readonly ICompressedSizeService encoder;
        private readonly uint minCompressionLevel;
        private readonly uint maxCompressionLevel;

        public CompressionLevelInferrer(
            ICompressedSizeService encoder,
            uint minCompressionLevel,
            uint maxCompressionLevel)
        {
            this.encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            this.minCompressionLevel = minCompressionLevel;

            if (maxCompressionLevel <= minCompressionLevel)
                throw new ArgumentOutOfRangeException($"{maxCompressionLevel} can't be smaller than {minCompressionLevel}");

            this.maxCompressionLevel = maxCompressionLevel;
        }

        /// <summary>
        /// Attempts to find a file's compression level by re-coding the file with different levels.
        /// Returns a range that either has a single value that represents a compression level, or has two values that the compression level is estimated to be between.
        /// </summary>
        /// <param name="targetCompressionLevel">A compression level to try first</param>
        public Range<uint> InferCompressionLevel(
            FileInfo sourceFile,
            uint targetCompressionLevel)
        {
            return InferCompressionLevel(sourceFile, targetCompressionLevel, 0);
        }        

        private Range<uint> InferCompressionLevel(
            FileInfo sourceFile,
            uint targetCompressionLevel,
            uint lastAttemptedCompressionLevel)
        {
            var size = encoder.GetCompressedSize(sourceFile, targetCompressionLevel);

            switch (CompareSize(sourceFile.Length, size))
            {
                case NumberDifference.Greater:
                    {
                        // compression levels exhausted. metadata in a file (if the encoder doesn't handle it properly) could interfere with numbers
                        if (targetCompressionLevel == minCompressionLevel)
                            return new Range<uint>(targetCompressionLevel, minCompressionLevel - 1);

                        uint nextCompresionLevel = targetCompressionLevel - 1;
                        if (nextCompresionLevel == lastAttemptedCompressionLevel)
                            return new Range<uint>(targetCompressionLevel, lastAttemptedCompressionLevel);

                        return InferCompressionLevel(sourceFile, nextCompresionLevel, targetCompressionLevel);
                    }
                case NumberDifference.Smaller:
                    {
                        // compression levels exhausted. this shouldn't ever happen
                        if (targetCompressionLevel == maxCompressionLevel)
                            return new Range<uint>(targetCompressionLevel, maxCompressionLevel + 1);

                        uint nextCompresionLevel = targetCompressionLevel + 1;
                        if (nextCompresionLevel == lastAttemptedCompressionLevel)
                            return new Range<uint>(targetCompressionLevel, lastAttemptedCompressionLevel);

                        return InferCompressionLevel(sourceFile, nextCompresionLevel, targetCompressionLevel);
                    }
                default:
                    return new Range<uint>(targetCompressionLevel);
            }
        }

        private NumberDifference CompareSize(long target, long other)
        {
            if (target < other) return NumberDifference.Smaller;
            if (target > other) return NumberDifference.Greater;

            return NumberDifference.Equal;
        }

        private enum NumberDifference
        {
            Smaller = -1,
            Equal = 0,
            Greater = 1
        }
    }
}