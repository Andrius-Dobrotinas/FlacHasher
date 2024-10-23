using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Application
{
    public static class Param
    {
        public const string DefaultHashfileExtension = "hash";

        public static string[] GetHashFileExtensions(string[] hashfileExtensions)
        {
            if (hashfileExtensions == null || !hashfileExtensions.Any())
                hashfileExtensions = new string[] { $".{DefaultHashfileExtension}" };

            return hashfileExtensions.Select(ext => $".{ext}").ToArray();
        }
    }
}
