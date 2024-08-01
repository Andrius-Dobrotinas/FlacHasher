using System;
using System.IO;
using System.Linq;
using System.Threading;
using Andy.FlacHash.IO;

namespace Andy.FlacHash.Audio.Flac.CmdLine
{
    public class FileDecoder : IAudioFileDecoder
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IOutputOnlyProcessRunner processRunner;

        public FileDecoder(FileInfo decoderExecutableFile,
            ExternalProcess.IOutputOnlyProcessRunner processRunner)
        {
            this.decoderExecutableFile = decoderExecutableFile ?? throw new ArgumentNullException(nameof(decoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public Stream Read(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            if (sourceFile.Exists == false)
                throw new SourceFileNotFoundException(sourceFile.FullName);

            var arguments = GetProcessArguments(sourceFile);

            return processRunner.RunAndReadOutput(
                decoderExecutableFile,
                arguments,
                cancellation);
        }

        private static string[] GetProcessArguments(FileInfo sourceFile)
        {
            return Parameters.Decode.File
                .Select(x =>
                    x.Equals(Parameters.FilePlaceholder, StringComparison.InvariantCultureIgnoreCase)
                        ? sourceFile.FullName
                        : x)
                .ToArray();
        }
    }
}