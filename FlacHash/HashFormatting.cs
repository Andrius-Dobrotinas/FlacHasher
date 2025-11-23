using System;

namespace Andy.FlacHash
{
    public static class HashFormatting
    {
        public static string GetInLowercase(byte[] hash)
        {
            return Convert.ToHexString(hash)
                .ToLowerInvariant();
        }
    }
}