using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Audio
{
    public abstract class IOException : System.IO.IOException
    {
        public IOException(string message)
            : base(message)
        {
        }

        public IOException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}