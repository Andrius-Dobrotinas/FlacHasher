using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Cmd
{
    public class InputFileMissingException : Exception
    {
        public InputFileMissingException(string msg) : base(msg)
        {
        }
    }
}
