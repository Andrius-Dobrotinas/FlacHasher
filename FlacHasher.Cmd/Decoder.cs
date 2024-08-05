using Andy.ExternalProcess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public static class AudioDecoderFactory
    {
        public static Audio.IAudioFileDecoder Build(FileInfo decoderFile, ProcessRunner processRunner, ICollection<string> args = null)
        {
            return args?.Contains(Audio.Parameter.FilePlaceholder) ?? false
                    ? BuildRegular(decoderFile, processRunner, args)
                    : BuildForStdin(decoderFile, processRunner, args ?? Audio.Flac.CmdLine.Parameters.Decode.Stream);
        }
        
        private static Audio.IAudioFileDecoder BuildRegular(FileInfo decoderFile, ProcessRunner processRunner, ICollection<string> args)
        {
            return new Audio.FileDecoder(
                    decoderFile,
                    processRunner,
                    args);
        }

        private static Audio.IAudioFileDecoder BuildForStdin(FileInfo decoderFile, ProcessRunner processRunner, ICollection<string> args)
        {
            var steamFactory = new IO.ReadStreamFactory();
            var decoder = new Audio.StreamDecoder(
                decoderFile,
                processRunner,
                args);
            return new Audio.AudioFileDecoder(steamFactory, decoder);
        }
    }
}