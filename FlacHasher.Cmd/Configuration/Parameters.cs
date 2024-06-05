using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public class Parameters
    {
        public string Decoder { get; set; }
        public string[] InputFiles { get; set; }
        public string InputDirectory { get; set; }
        public string TargetFileExtension { get; set; }
        public string OutputFormat { get; set; }
        public int? ProcessExitTimeoutMs { get; set; }
        public int? ProcessTimeoutSec { get; set; }
        public bool? FailOnError { get; set; }
    }
}