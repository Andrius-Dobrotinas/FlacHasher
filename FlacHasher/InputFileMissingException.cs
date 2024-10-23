using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application
{
    public class InputFileMissingException : Exception
    {
        public InputFileMissingException(string msg) : base(msg)
        {
        }
    }
}
