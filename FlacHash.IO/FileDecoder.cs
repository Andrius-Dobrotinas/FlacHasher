using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Andy.FlacHash.Audio
{
    public class FileDecoder : IAudioFileDecoder
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IOutputOnlyProcessRunner processRunner;
        private readonly ICollection<string> @params;

        public FileDecoder(
            FileInfo decoderExecutableFile,
            ExternalProcess.IOutputOnlyProcessRunner processRunner,
            ICollection<string> @params)
        {
            this.decoderExecutableFile = decoderExecutableFile ?? throw new ArgumentNullException(nameof(decoderExecutableFile));
            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            this.@params = @params ?? throw new ArgumentNullException(nameof(@params));
        }

        public Stream Read(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            if (sourceFile.Exists == false)
                throw new InputFileNotFoundException(sourceFile.FullName);

            var arguments = GetProcessArguments(sourceFile);

            return processRunner.RunAndReadOutput(
                decoderExecutableFile,
                arguments,
                cancellation);
        }

        private string[] GetProcessArguments(FileInfo sourceFile)
        {
            return @params
                .Select(x =>
                    x.Equals(Parameter.FilePlaceholder, StringComparison.InvariantCultureIgnoreCase)
                        ? sourceFile.FullName
                        : x)
                .ToArray();
        }
    }
}