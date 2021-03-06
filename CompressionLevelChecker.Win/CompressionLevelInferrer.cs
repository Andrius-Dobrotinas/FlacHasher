﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public class CompressionLevelInferrer
    {
        private readonly ICompressedSizeService encoder;
        private readonly IFileInfoSizeGetter fileSizeGetter;
        private readonly int minCompressionLevel;
        private readonly int maxCompressionLevel;

        public CompressionLevelInferrer(
            ICompressedSizeService encoder,
            IFileInfoSizeGetter fileSizeGetter,
            int minCompressionLevel,
            int maxCompressionLevel)
        {
            this.encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            this.fileSizeGetter = fileSizeGetter ?? throw new ArgumentNullException(nameof(fileSizeGetter));

            this.minCompressionLevel = minCompressionLevel;

            if (maxCompressionLevel < minCompressionLevel)
                throw new ArgumentOutOfRangeException("Max compression level cannot be lower than Min");

            this.maxCompressionLevel = maxCompressionLevel;
        }

        /// <summary>
        /// Attempts to find a file's compression level by re-coding the file with different levels.
        /// Returns a range that either has a single value that represents a compression level, or has two values that the compression level is estimated to be between.
        /// </summary>
        /// <param name="targetCompressionLevel">A compression level to try first</param>
        public Range<int> InferCompressionLevel(
            FileInfo sourceFile,
            int targetCompressionLevel)
        {
            if (targetCompressionLevel > maxCompressionLevel)
                throw new ArgumentOutOfRangeException(nameof(targetCompressionLevel), "Target compression level cannot be greater than max level");

            if (targetCompressionLevel < minCompressionLevel)
                throw new ArgumentOutOfRangeException(nameof(targetCompressionLevel), "Target compression level cannot be lower than min level");

            return InferCompressionLevel(sourceFile, targetCompressionLevel, 0);
        }        

        private Range<int> InferCompressionLevel(
            FileInfo sourceFile,
            int targetCompressionLevel,
            int lastAttemptedCompressionLevel)
        {
            var recodedSize = encoder.GetCompressedSize(sourceFile, targetCompressionLevel);
            long originalSize = fileSizeGetter.GetSize(sourceFile);

            switch (CompareSize(originalSize, recodedSize))
            {
                case NumberDifference.Greater:
                    {
                        // compression levels exhausted. metadata in a file (if the encoder doesn't handle it properly) could interfere with numbers
                        if (targetCompressionLevel == minCompressionLevel)
                            return new Range<int>(targetCompressionLevel, minCompressionLevel + 1);

                        int nextCompresionLevel = targetCompressionLevel - 1;
                        if (nextCompresionLevel == lastAttemptedCompressionLevel)
                            return new Range<int>(lastAttemptedCompressionLevel, targetCompressionLevel);

                        return InferCompressionLevel(sourceFile, nextCompresionLevel, targetCompressionLevel);
                    }
                case NumberDifference.Smaller:
                    {
                        // compression levels exhausted. this shouldn't ever happen
                        if (targetCompressionLevel == maxCompressionLevel)
                            return new Range<int>(targetCompressionLevel, maxCompressionLevel + 1);

                        int nextCompresionLevel = targetCompressionLevel + 1;
                        if (nextCompresionLevel == lastAttemptedCompressionLevel)
                            return new Range<int>(targetCompressionLevel, lastAttemptedCompressionLevel);

                        return InferCompressionLevel(sourceFile, nextCompresionLevel, targetCompressionLevel);
                    }
                default:
                    return new Range<int>(targetCompressionLevel);
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