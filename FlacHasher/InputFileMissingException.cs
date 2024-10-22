using System;
using System.Collections.Generic;

namespace Andy.FlacHash
{
    public class InputFileMissingException : Exception
    {
        public InputFileMissingException(string msg) : base(msg)
        {
        }
    }
}
