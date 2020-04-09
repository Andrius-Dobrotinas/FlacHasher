using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Input
{
    /// <summary>
    /// Indicates that input could not be read
    /// </summary>
    public class InputReadingException : Exception
    {
        public InputReadingException(string message)
            : base(message)
        {

        }
    }
}