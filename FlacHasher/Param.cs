using Andy.FlacHash.Hashing.Verification;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash
{
    public static class Param
    {
        public static string[] GetHashFileExtensions(string[] hashfileExtensions)
        {
            if (hashfileExtensions == null || !hashfileExtensions.Any())
                hashfileExtensions = new string[] { $".{FileHashMap.DefaultExtension}" };

            return hashfileExtensions.Select(ext => $".{ext}").ToArray();
        }
    }
}
