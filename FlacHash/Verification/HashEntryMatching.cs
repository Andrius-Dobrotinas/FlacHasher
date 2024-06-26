using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public static class HashEntryMatching
    {
        public const string MissingFileKey = "{{File {0}}}";

        /// <summary>
        /// Matches <paramref name="files"/> with <paramref name="expectedHashes"/> based on their positions, not file name.
        /// For <paramref name="expectedHashes"/> that don't have a corresponding file, returns <see cref="FileInfo"/>s with
        /// <see cref="MissingFileKey"/> with expected file position as file name and "current" working dir path.
        /// </summary>
        /// <param name="expectedHashes">Key = File name, Value = hash value</param>
        public static IEnumerable<KeyValuePair<FileInfo, string>> MatchFilesToHashesPositionBased(
            IList<KeyValuePair<string, string>> expectedHashes, 
            IEnumerable<FileInfo> files)
        {
            var filesTargetedByTheHashes = files.Take(expectedHashes.Count).ToList();

            for (int i = 0; i < expectedHashes.Count; i++)
            {
                var expected = expectedHashes[i];

                var matchingFile = (i < filesTargetedByTheHashes.Count)
                    ? filesTargetedByTheHashes[i]
                    : null;

                yield return new KeyValuePair<FileInfo, string>(
                        matchingFile ?? new FileInfo(string.Format(MissingFileKey, i+1)),
                        expected.Value);
            }
        }

        /// <summary>
        /// Matches <paramref name="files"/> with <paramref name="expectedHashes"/> based on file name.
        /// For <paramref name="expectedHashes"/> that don't have a corresponding file, returns a <see cref="FileInfo"/>s with
        /// the expected file name with "current" working dir path.
        /// </summary>
        /// <param name="expectedHashes">Key = File name, Value = hash value</param>
        public static IEnumerable<KeyValuePair<FileInfo, string>> MatchFilesToHashesNameBased(
            IList<KeyValuePair<string, string>> expectedHashes, 
            IEnumerable<FileInfo> files)
        {
            var fileDictionary = files.ToDictionary(x => x.Name, x => x);

            for (int i = 0; i < expectedHashes.Count; i++)
            {
                var expected = expectedHashes[i];
                var matchingFile = fileDictionary.GetValueOrDefault(expected.Key);

                yield return new KeyValuePair<FileInfo, string>(
                        matchingFile ?? new FileInfo(expected.Key),
                        expected.Value);
            }
        }

        /// <summary>
        /// Matches <paramref name="files"/> with <paramref name="expectedFileHashMap"/> depending on how <paramref name="expectedFileHashMap"/> is defined.
        /// </summary>
        public static IEnumerable<KeyValuePair<FileInfo, string>> MatchFilesToHashes(FileHashMap expectedFileHashMap, IList<FileInfo> files)
        {
            return expectedFileHashMap.IsPositionBased
                    ? HashEntryMatching.MatchFilesToHashesPositionBased(expectedFileHashMap.Hashes, files)
                    : HashEntryMatching.MatchFilesToHashesNameBased(expectedFileHashMap.Hashes, files);
        }
    }
}
