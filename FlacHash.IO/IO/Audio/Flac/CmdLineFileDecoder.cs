using Andy.FlacHash.ExternalProcess;
using System;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac
{
    public class CmdLineFileDecoder : IFileReader
    {
        private readonly FileInfo decoderExecutableFile;
        private readonly IOutputOnlyProcessRunner processRunner;

        public CmdLineFileDecoder(FileInfo decoderExecutableFile,
            IOutputOnlyProcessRunner processRunner)
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
                var message = $"Failed to decode the file. FLAC process exited with error code {e.ExitCode}.\nFLAC error output: {e.ProcessErrorOutput}";

                throw new InputReadingException(message);
            }
        }

        private static string[] GetProcessArguments(FileInfo sourceFile)
        {
            return new string[]
            {
                CmdLineDecoderOptions.Decode,
                CmdLineDecoderOptions.WriteToSdtOut,
                sourceFile.FullName
            };
        }
    }
}