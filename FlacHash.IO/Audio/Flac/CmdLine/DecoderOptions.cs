using System;

namespace Andy.FlacHash.Audio.Flac.CmdLine
{
    public static class DecoderOptions
    {
        public const string Decode = "--decode";
        public const string WriteToSdtOut = "--stdout";
        public const string ReadFromStdIn = "-"; //hardly a documentated feature, discoverd it by semi-accident
    }
}