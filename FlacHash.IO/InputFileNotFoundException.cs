using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Andy.FlacHash.IO
{
    public class InputFileNotFoundException : Exception
    {
        public string Filename { get; }

        public InputFileNotFoundException(string filename)
            : base($"File not found: {filename}")
        {
            Filename = filename;
        }
    }
}
