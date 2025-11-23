using System;

namespace Andy.FlacHash.Application.Win
{
    public class DecoderProfileIniSection : DecoderProfile
    {
        [IniEntry(nameof(Decoder))]
        [Cmd.Parameters.DecoderExeDescription]
        public override string Decoder { get; set; }

        [IniEntry(nameof(DecoderParameters))]
        [Cmd.Parameters.DecoderParamsDescription]
        public override string[] DecoderParameters { get; set; }

        [IniEntry(nameof(TargetFileExtensions))]
        [Cmd.Parameters.DecoderTargetFileExtensions]
        public override string[] TargetFileExtensions { get; set; }
    }
}
