using System;
using System.Collections.Generic;

namespace Andy.FlacHash.IO
{
    public class InputFileNotFoundException : IOException
    {
        public string Filename { get; }

        public InputFileNotFoundException(string filename)
            : base($"File not found: {filename}")
        {
            Filename = filename;
        }
    }
}
