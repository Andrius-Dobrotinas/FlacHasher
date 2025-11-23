using System;

namespace Andy.FlacHash.Application.Win
{
    public class DecoderProfileIniSection : DecoderProfile
    {
        [IniEntry(nameof(Decoder))]
        [DecoderExeDescription]
        public override string Decoder { get; set; }

        [IniEntry(nameof(DecoderParameters))]
        [DecoderParamsDescription]
        public override string[] DecoderParameters { get; set; }

        [IniEntry(nameof(TargetFileExtensions))]
        [DecoderTargetFileExtensions]
        public override string[] TargetFileExtensions { get; set; }
    }
}
