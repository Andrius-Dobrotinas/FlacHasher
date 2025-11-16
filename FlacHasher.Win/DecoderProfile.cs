using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Application.Win
{
    public class DecoderProfile
    {
        public string Name { get; set; }

        [Cmd.Parameters.DecoderExeDescription]
        public virtual string Decoder { get; set; }
        
        [Cmd.Parameters.DecoderParamsDescription]
        public virtual string[] DecoderParameters { get; set; }
        
        [Cmd.Parameters.DecoderTargetFileExtensions]
        public virtual string[] TargetFileExtensions { get; set; }
    }
}