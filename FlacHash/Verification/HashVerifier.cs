using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class HashVerifier
    {
        public bool DoesMatch(IDictionary<FileInfo, string> expectedHashes, FileInfo file, byte[] actualHash)
        {
            var targetHash = Convert.FromHexString(expectedHashes[file]);
            var isMatch = targetHash.SequenceEqual(actualHash);

            return isMatch;
        }
    }
}