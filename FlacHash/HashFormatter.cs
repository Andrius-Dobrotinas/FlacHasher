using System;

namespace Andy.FlacHash
{
    public class HashFormatter : IHashFormatter
    {
        public string GetString(byte[] hash)
        {
            return HashFormatting.GetInLowercase(hash);
        }
    }
}