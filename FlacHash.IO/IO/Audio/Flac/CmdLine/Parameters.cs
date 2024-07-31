using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Andy.FlacHash.IO.Audio.Flac.CmdLine
{
    public static class Parameters
    {
        public const string FilePlaceholder = "{file}";

        public static class Decode
        {
            public static ICollection<string> Stream = new string[]
            {
                DecoderOptions.Decode,
                DecoderOptions.ReadFromStdIn
            };

            public static ICollection<string> File = new string[]
            {
                DecoderOptions.Decode,
                DecoderOptions.WriteToSdtOut,
                FilePlaceholder
            };
        }
    }
}
