using Andy.ExternalProcess;
using Andy.FlacHash.Audio;
using Andy.FlacHash.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Application.Audio
{
    public static class AudioDecoder
    {

        /// <summary>
        /// Builds an Audio decoder based on whether input file is referenced by the parameters; otherwise, it's assumed the data will be fed to the decoder via stdin.
        /// </summary>
        /// <param name="inputStreamReadProgressReporter">Optional and only applicable when data is to be fed via stdin</param>
        /// <returns></returns>
        public static IAudioFileDecoder Build(FileInfo decoderFile, ProcessRunner processRunner, ICollection<string> args, IProgress<int> inputStreamReadProgressReporter = null)
        {
            return AudioFileDecoder.ContainsFilePlaceholder(args)
                    ? BuildRegular(decoderFile, processRunner, args)
                    : BuildForStdin(decoderFile, processRunner, args, inputStreamReadProgressReporter);
        }

        public static IAudioFileDecoder BuildRegular(FileInfo decoderFile, ProcessRunner processRunner, ICollection<string> args)
        {
            return new AudioFileDecoder(
                    decoderFile,
                    processRunner,
                    args);
        }

        public static IAudioFileDecoder BuildForStdin(FileInfo decoderFile, ProcessRunner processRunner, ICollection<string> args, IProgress<int> inputStreamReadProgressReporter = null)
        {
            var streamFactory = inputStreamReadProgressReporter == null
                ? (IFileReadStreamFactory)new ReadStreamFactory()
                : new ProgressReportingReadStreamFactory(inputStreamReadProgressReporter);
            var decoder = new StreamDecoder(
                decoderFile,
                processRunner,
                args);
            return new StdInputStreamAudioFileDecoder(streamFactory, decoder);
        }

        public static ICollection<string> GetDefaultDecoderParametersIfEmpty(ICollection<string> @params, FileInfo decoderFile)
        {
            return @params != null && @params.Any()
                ? @params
                : IsFlac(decoderFile)
                    ? Flac.Parameters.Decode.Stream
                    : throw new ConfigurationException("Decoder Parameters must be provided for a decoder other than FLAC");
        }

        public static bool IsFlac(FileInfo decoderExe)
        {
            return decoderExe.Name.Contains(Flac.FormatMetadata.DecoderExeName, StringComparison.InvariantCultureIgnoreCase);
        }

        static FileInfo FindDecoderInPaths(string decoderFilename, IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, decoderFilename);
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

        public static FileInfo ResolveDecoderOrThrow(ApplicationSettings settings)
        {
            return ResolveDecoder(settings.DecoderExe)
                ?? throw new ConfigurationException($"The specified decoder exe file was not found (not in PATH either): {settings.DecoderExe}");
        }
    }
}