using System;
using System.IO;

namespace Andy.FlacHash
{
    public class OutputFormatter
    {
        public static string GetFormattedString(string format, byte[] hash, FileInfo file)
        {
            return format
                .Replace("{hash}", BitConverter.ToString(hash), StringComparison.InvariantCulture)
                .Replace("{name}", file.Name, StringComparison.InvariantCulture)
                .Replace("{path}", file.FullName, StringComparison.InvariantCulture);
        }
    }
}