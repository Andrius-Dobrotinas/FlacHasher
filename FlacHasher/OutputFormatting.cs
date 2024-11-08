using System;
using System.IO;

namespace Andy.FlacHash.Application
{
    public class OutputFormatting
    {
        public static string GetFormattedString(string format, string hash, FileInfo file)
        {
            return format
                .Replace("{hash}", hash, StringComparison.InvariantCulture)
                .Replace("{name}", file.Name, StringComparison.InvariantCulture)
                .Replace("{path}", file.FullName, StringComparison.InvariantCulture);
        }
    }
}