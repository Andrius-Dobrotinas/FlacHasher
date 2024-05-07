using System;
using System.Collections.Generic;
using System.IO;
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

        public bool DoesMatch(IList<KeyValuePair<FileInfo, string>> expectedHashes, int hashIndex, byte[] actualHash)
        {
            var targetHash = expectedHashes[hashIndex].Value;
            var actualHashString = hashFormatter.GetString(actualHash);
            var isMatch = AreEqualOrdinalCaseInsensitive(targetHash, actualHashString);

            return isMatch;
        }

        private static bool AreEqualOrdinalCaseInsensitive(string one, string two)
        {
            return string.Equals(one, two, StringComparison.OrdinalIgnoreCase);
        }
    }
}