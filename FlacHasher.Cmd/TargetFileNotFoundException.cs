using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Cmd
{
    public class TargetFileNotFoundException : Exception
    {
        public TargetFileNotFoundException(string msg) : base(msg)
        {
        }
    }
}
