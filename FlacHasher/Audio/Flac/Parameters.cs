using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Andy.FlacHash.Application.Audio.Flac
{
    public static class Parameters
    {
        public static class Options
        {
            public static class Decoder
            {
                public const string Decode = "--decode";
                public const string WriteToSdtOut = "--stdout";
                public const string ReadFromStdIn = "-"; //hardly a documentated feature, discoverd it by semi-accident
            }

            public static class Encoder
            {
                public const string Stdout = "--stdout";
            }
        }

        public static class Decode
        {
            public static ICollection<string> Stream = new string[]
            {
                Options.Decoder.Decode,
                Options.Decoder.ReadFromStdIn
            };

            public static ICollection<string> File = new string[]
            {
                Options.Decoder.Decode,
                Options.Decoder.WriteToSdtOut,
                FlacHash.Audio.DecoderParameter.FilePlaceholder
            };
        }
    }
}
