using System;
using System.IO;
using Andy.FlacHash.Hashing;

namespace Andy.FlacHash.Cmd
{
    public class OutputFormatting
    {
        public static string GetFormattedString(string format, byte[] hash, FileInfo file)
        {
            return format
                .Replace("{hash}", HashFormatting.GetInLowercase(hash), StringComparison.InvariantCulture)
                .Replace("{name}", file.Name, StringComparison.InvariantCulture)
                .Replace("{path}", file.FullName, StringComparison.InvariantCulture);
        }
    }
}