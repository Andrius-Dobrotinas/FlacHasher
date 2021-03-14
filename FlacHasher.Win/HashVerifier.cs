using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Win
{
    public class HashVerifier
    {
        private readonly IHashFormatter hashFormatter;

        public HashVerifier(IHashFormatter hashFormatter)
        {
            this.hashFormatter = hashFormatter;
        }

        public bool DoesMatch(IList<KeyValuePair<string, string>> expectedHashes, FileHashResult actualHash, int fileIndex)
        {
            var hash = hashFormatter.GetString(actualHash.Hash);
            var isMatch = AreEqualOrdinalCaseInsensitive(expectedHashes[fileIndex].Value, hash);

            return isMatch;
        }

        public bool DoesMatch(
            IList<KeyValuePair<string, string>> expectedHashes,
            FileHashResult actual)
        {
            var fileName = actual.File.Name;

            var expected = expectedHashes.First(x => AreEqualOrdinalCaseInsensitive(x.Key, fileName));

            var actualHash = hashFormatter.GetString(actual.Hash);

            bool isMatch = AreEqualOrdinalCaseInsensitive(actualHash, expected.Value);

            return isMatch;
        }

        private static bool AreEqualOrdinalCaseInsensitive(string one, string two)
        {
            return string.Equals(one, two, StringComparison.OrdinalIgnoreCase);
        }
    }
}