using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Andy.FlacHash.Audio
{
    /// <summary>
    /// Sends file name to the decoder via parameters
    /// </summary>
    public class AudioFileDecoder : IAudioFileDecoder
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IOutputOnlyProcessRunner processRunner;
        private readonly ICollection<string> @params;

        public AudioFileDecoder(
            FileInfo decoderExecutableFile,
            ExternalProcess.IOutputOnlyProcessRunner processRunner,
            ICollection<string> @params)
        {
            this.decoderExecutableFile = decoderExecutableFile ?? throw new ArgumentNullException(nameof(decoderExecutableFile));
            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            this.@params = @params ?? throw new ArgumentNullException(nameof(@params));

            if (!ContainsFilePlaceholder(@params))
                throw new ArgumentException($"One of the parameters has to be a file placeholder: {Parameter.FilePlaceholder}", nameof(@params));
        }

        public DecoderStream Read(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            if (sourceFile.Exists == false)
                throw new InputFileNotFoundException(sourceFile.FullName);

            var arguments = GetProcessArguments(sourceFile);

            return new DecoderStream(processRunner.RunAndReadOutput(
                decoderExecutableFile,
                arguments,
                cancellation));
        }

        private string[] GetProcessArguments(FileInfo sourceFile)
        {
            return @params
                .Select(param =>
                    param.Equals(Parameter.FilePlaceholder, StringComparison.InvariantCultureIgnoreCase)
                        ? sourceFile.FullName
                        : param)
                .ToArray();
        }

        public static bool ContainsFilePlaceholder(IEnumerable<string> @params)
        {
            return @params.Any(param => param.Equals(Parameter.FilePlaceholder, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}