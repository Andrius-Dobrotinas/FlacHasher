using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class HashVerifier
    {
        private readonly IHashFormatter hashFormatter;

        public HashVerifier(IHashFormatter hashFormatter)
        {
            this.hashFormatter = hashFormatter;
        }

        public bool DoesMatch(IDictionary<FileInfo, string> expectedHashes, FileInfo file, byte[] actualHash)
        {
            var targetHash = expectedHashes[file];
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