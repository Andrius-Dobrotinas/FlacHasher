using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Input
{
    /// <summary>
    /// Indicates that input could not be read
    /// </summary>
    public class InputReadException : Exception
    {
        public InputReadException(string message)
            : base(message)
        {

        }
    }
}