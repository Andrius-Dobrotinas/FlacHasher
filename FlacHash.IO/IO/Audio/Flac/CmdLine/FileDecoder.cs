using System;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac.CmdLine
{
    public class FileDecoder : IFileReader
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly ExternalProcess.IOutputOnlyProcessRunner processRunner;

        public FileDecoder(FileInfo decoderExecutableFile,
            ExternalProcess.IOutputOnlyProcessRunner processRunner)
        {
            this.decoderExecutableFile = decoderExecutableFile ?? throw new ArgumentNullException(nameof(decoderExecutableFile));

            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public Stream Read(FileInfo sourceFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            var arguments = GetProcessArguments(sourceFile);

            try
            {
                return processRunner.RunAndReadOutput(
                    decoderExecutableFile,
                    arguments);
            }
            catch(ExternalProcess.ExecutionException e)
            {
                throw new FlacCompressionException("Failed to decode the file", e);
            }
        }

        private static string[] GetProcessArguments(FileInfo sourceFile)
        {
            return new string[]
            {
                DecoderOptions.Decode,
                DecoderOptions.WriteToSdtOut,
                sourceFile.FullName
            };
        }
    }
}