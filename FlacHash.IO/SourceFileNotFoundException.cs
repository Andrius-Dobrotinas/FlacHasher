using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Andy.FlacHash.IO
{
    public class SourceFileNotFoundException : Exception
    {
        public string Filename { get; }

        public SourceFileNotFoundException(string filename)
            : base($"File not found: {filename}")
        {
            Filename = filename;
        }
    }
}
