using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Application
{
    public static class FileExtension
    {
        public static string[] PrefixWithDot(IEnumerable<string> extensions)
        {
            return extensions.Select(ext => $".{ext}").ToArray();
        }
    }
}
