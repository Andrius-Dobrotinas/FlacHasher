using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Audio
{
    public abstract class IOException : Exception
    {
        public IOException(string message)
            : base(message)
        {

        }
    }
}