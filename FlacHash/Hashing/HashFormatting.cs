using System;

namespace Andy.FlacHash.Hashing
{
    public static class HashFormatting
    {
        public static string GetInLowercase(byte[] hash)
        {
            return BitConverter.ToString(hash)
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }
}