using Andy.ExternalProcess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public static class AudioDecoder
    {
        public static Audio.IAudioFileDecoder Build(FileInfo decoderFile, ProcessRunner processRunner, ICollection<string> args)
        {
            return args.Any(arg => arg.Equals(Audio.Parameter.FilePlaceholder, StringComparison.InvariantCultureIgnoreCase))
                    ? BuildRegular(decoderFile, processRunner, args)
                    : BuildForStdin(decoderFile, processRunner, args);
        }
        
        private static Audio.IAudioFileDecoder BuildRegular(FileInfo decoderFile, ProcessRunner processRunner, ICollection<string> args)
        {
            return new Audio.AudioFileDecoder(
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
            return new Audio.StdInputStreamAudioFileDecoder(steamFactory, decoder);
        }

        public static ICollection<string> GetDecoderParametersOrDefault(ICollection<string> @params, FileInfo decoderFile)
        {
            return @params ??
                    (IsFlac(decoderFile)
                        ? Audio.Flac.Parameters.Decode.Stream
                        : throw new ConfigurationException("Decoder Parameters must be provided for a decoder other than FLAC"));
        }

        public static bool IsFlac(FileInfo decoderExe)
        {
            return decoderExe.Name.Contains(Audio.Flac.FormatMetadata.DecoderExeName, StringComparison.InvariantCultureIgnoreCase);
        }

        static FileInfo FindDecoderInPaths(string decoderPath, IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, decoderPath);
                if (File.Exists(fullPath))
                    return new FileInfo(fullPath);
            }

            return null;
        }

        public static FileInfo ResolveDecoder(string decoderPath)
        {
            // If it's a relative path but the decoder is in the same directory as this program, then the decoder will be found
            if (File.Exists(decoderPath))
                return new FileInfo(decoderPath);

            if (Path.IsPathRooted(decoderPath))
                return null;

            var paths = Environment.GetEnvironmentVariable("PATH").Split(';');
            return FindDecoderInPaths(decoderPath, paths);
        }
    }
}