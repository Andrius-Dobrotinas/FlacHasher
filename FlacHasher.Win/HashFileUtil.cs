using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Win
{
    public static class HashFileUtil
    {
        public const string MissingFileKey = "File's Missing";

        /// <param name="expectedHashes">Key = File name, Value = hash value</param>
        public static (
            IList<KeyValuePair<FileInfo, string>> expected, 
            IList<KeyValuePair<FileInfo, string>> missing) 
            GetHashDataPositionBased(
            IList<KeyValuePair<string, string>> expectedHashes, 
            IEnumerable<FileInfo> files)
        {
            var filesTargetedByTheHashes = files.Take(expectedHashes.Count).ToList();

            var result = new List<KeyValuePair<FileInfo, string>>();
            var missing = new List<KeyValuePair<FileInfo, string>>();

            for (int i = 0; i < expectedHashes.Count; i++)
            {
                var expected = expectedHashes[i];

                var fileOnFileSystem = (i < filesTargetedByTheHashes.Count)
                    ? filesTargetedByTheHashes[i]
                    : null;

                if (fileOnFileSystem != null)
                    result.Add(
                        new KeyValuePair<FileInfo, string>(fileOnFileSystem, expected.Value));
                else
                    missing.Add(
                        new KeyValuePair<FileInfo, string>(
                            new FileInfo("File's Missing"),
                            expected.Value));
            }

            return (result, missing);
        }

        /// <param name="expectedHashes">Key = File name, Value = hash value</param>
        public static (
            IList<KeyValuePair<FileInfo, string>> expected, 
            IList<KeyValuePair<FileInfo, string>> missing) 
            GetHashData(
            IList<KeyValuePair<string, string>> expectedHashes, 
            IEnumerable<FileInfo> files)
        {
            var nameToFileDictionary = files.ToDictionary(x => x.Name, x => x);

            var result = new List<KeyValuePair<FileInfo, string>>();
            var missing = new List<KeyValuePair<FileInfo, string>>();

            for (int i = 0; i < expectedHashes.Count; i++)
            {
                var expected = expectedHashes[i];
                var fileOnFileSystem = nameToFileDictionary.GetValueOrDefault(expected.Key);

                if (fileOnFileSystem != null)
                    result.Add(
                        new KeyValuePair<FileInfo, string>(fileOnFileSystem, expected.Value));
                else
                    missing.Add(
                        new KeyValuePair<FileInfo, string>(
                            new FileInfo(expected.Key),
                            expected.Value));
            }

            return (result, missing);
        }
    }
}
