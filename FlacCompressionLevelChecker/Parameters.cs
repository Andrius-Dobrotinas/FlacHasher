using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash
{
    public class Parameters
    {
        public string FlacExec { get; set; }
        public string SourceFile { get; set; }
        public uint? CompressionLevel { get; set; }
    }
}