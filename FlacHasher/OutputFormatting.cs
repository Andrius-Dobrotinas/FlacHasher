using System;
using System.IO;

namespace Andy.FlacHash.Application
{
    public class OutputFormatting
    {
        public static class Placeholders
        {
            public const string Hash = "{hash}";
            public const string FileName = "{name}";
            public const string FilePath = "{path\\name}";
        }

        public static string GetFormattedString(string format, string hash, FileInfo file)
        {
            return format
                .Replace(Placeholders.Hash, hash, StringComparison.InvariantCulture)
                .Replace(Placeholders.FileName, file.Name, StringComparison.InvariantCulture)
                .Replace(Placeholders.FilePath, file.FullName, StringComparison.InvariantCulture);
        }
    }
}