using System;

namespace Andy.FlacHash.Hashfile.Read
{
    public class InvalidHashLineFormatException : HashEntryException
    {
        public InvalidHashLineFormatException(string msg) : base(msg)
        {
        }
    }
}
