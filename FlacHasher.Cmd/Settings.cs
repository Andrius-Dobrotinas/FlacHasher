using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public class Settings
    {
        public FileInfo Decoder { get; set; }
        public string OutputFormat { get; set; }
        public int? ProcessExitTimeoutMs { get; set; }
        public int? ProcessTimeoutSec { get; set; }
        public bool? FailOnError { get; set; }
    }
}