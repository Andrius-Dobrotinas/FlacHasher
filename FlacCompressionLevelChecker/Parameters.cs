using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.CompressionLevel
{
    public class Parameters
    {
        public string FlacExec { get; set; }
        public string SourceFile { get; set; }
        public int? CompressionLevel { get; set; }
    }
}