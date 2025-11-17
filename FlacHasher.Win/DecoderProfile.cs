using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Application.Win
{
    public class DecoderProfile
    {
        public string Name { get; set; }

        [DecoderExeDescription]
        public virtual string Decoder { get; set; }
        
        [DecoderParamsDescription]
        public virtual string[] DecoderParameters { get; set; }
        
        [DecoderTargetFileExtensions]
        public virtual string[] TargetFileExtensions { get; set; }
    }
}