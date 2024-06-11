using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public static class HashEntryMatching
    {
        public const string MissingFileKey = "{FILE_MISSING}";

        /// <summary>
        /// Matches <paramref name="files"/> with <paramref name="expectedHashes"/> based on their position, not file name.
        /// For each file that's not found, return file info with <see cref="MissingFileKey"/> as file name.
        /// </summary>
        /// <param name="expectedHashes">Key = File name, Value = hash value</param>
        public static (
            IList<KeyValuePair<FileInfo, string>> present, 
            IList<KeyValuePair<FileInfo, string>> missing)
            MatchFilesToHashesPositionBased(
            IList<KeyValuePair<string, string>> expectedHashes, 
            IEnumerable<FileInfo> files)
        {
            var filesTargetedByTheHashes = files.Take(expectedHashes.Count).ToList();

            var result = new List<KeyValuePair<FileInfo, string>>();
            var missing = new List<KeyValuePair<FileInfo, string>>();

            for (int i = 0; i < expectedHashes.Count; i++)
            {
                var expected = expectedHashes[i];

                var matchingFile = (i < filesTargetedByTheHashes.Count)
                    ? filesTargetedByTheHashes[i]
                    : null;

                if (matchingFile != null)
                    result.Add(
                        new KeyValuePair<FileInfo, string>(matchingFile, expected.Value));
                else
                    missing.Add(
                        new KeyValuePair<FileInfo, string>(
                            new FileInfo(MissingFileKey),
                            expected.Value));
            }

            return (result, missing);
        }

        /// <summary>
        /// Matches <paramref name="files"/> with <paramref name="expectedHashes"/> based on file name.
        /// </summary>
        /// <param name="expectedHashes">Key = File name, Value = hash value</param>
        public static (
            IList<KeyValuePair<FileInfo, string>> present,
            IList<KeyValuePair<FileInfo, string>> missing)
            MatchFilesToHashesNameBased(
            IList<KeyValuePair<string, string>> expectedHashes, 
            IEnumerable<FileInfo> files)
        {
            var fileDictionary = files.ToDictionary(x => x.Name, x => x);

            var result = new List<KeyValuePair<FileInfo, string>>();
            var missing = new List<KeyValuePair<FileInfo, string>>();

            for (int i = 0; i < expectedHashes.Count; i++)
            {
                var expected = expectedHashes[i];
                var matchingFile = fileDictionary.GetValueOrDefault(expected.Key);

                if (matchingFile != null)
                    result.Add(
                        new KeyValuePair<FileInfo, string>(matchingFile, expected.Value));
                else
                    missing.Add(
                        new KeyValuePair<FileInfo, string>(
                            new FileInfo(expected.Key),
                            expected.Value));
            }

            return (result, missing);
        }

        /// <summary>
        /// Matches <paramref name="files"/> with <paramref name="expectedFileHashMap"/> depending on how <paramref name="expectedFileHashMap"/> is defined.
        /// </summary>
        public static (
            IList<KeyValuePair<FileInfo, string>> present,
            IList<KeyValuePair<FileInfo, string>> missing)
            MatchFilesToHashes(FileHashMap expectedFileHashMap, IList<FileInfo> files)
        {
            return expectedFileHashMap.IsPositionBased
                    ? HashEntryMatching.MatchFilesToHashesPositionBased(expectedFileHashMap.Hashes, files)
                    : HashEntryMatching.MatchFilesToHashesNameBased(expectedFileHashMap.Hashes, files);
        }
    }
}
