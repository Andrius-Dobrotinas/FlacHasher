using System;

namespace Andy.FlacHash
{
    /// <summary>
    /// Represents hash bytes as a lower-case string without any dashes or anything
    /// </summary>
    public class PlainLowercaseHashFormatter : IHashFormatter
    {
        public string GetString(byte[] hash)
        {
            return HashFormatting.GetInLowercase(hash);
        }
    }
}