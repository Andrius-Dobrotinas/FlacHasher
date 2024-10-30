using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Application.Win
{
    public class DecoderProfile
    {
        public string Name { get; set; }
        public FileInfo Decoder { get; set; }
        public string[] DecoderParameters { get; set; }
        public string TargetFileExtension { get; set; }
    }
}