﻿using System;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public class OutputFormatter
    {
        public static string GetFormattedString(string format, byte[] hash, FileInfo file)
        {
            return format
                .Replace("{hash}", BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(), StringComparison.InvariantCulture)
                .Replace("{name}", file.Name, StringComparison.InvariantCulture)
                .Replace("{path}", file.FullName, StringComparison.InvariantCulture);
        }
    }
}