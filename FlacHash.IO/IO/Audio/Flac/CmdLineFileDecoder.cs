using Andy.FlacHash.ExternalProcess;
using System;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.IO.Audio.Flac
{
    // TODO: Flac.exe can also take input via stdin. See if I want to go that way.

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
            var processSettings = GetProcessSettings(sourceFile, decoderExecutableFile);

            try
            {
                return processRunner.RunAndReadOutput(processSettings);
            }
            catch(ExternalProcess.ExecutionException e)
            {
                var message = $"Failed to read the input file. FLAC process exited with error code {e.ExitCode}.\nFLAC error output: {e.ProcessErrorOutput}";

                throw new InputReadingException(message);
            }
        }

        private static ProcessStartInfo GetProcessSettings(FileInfo sourceFile, FileInfo decoderExecutableFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));

            var processSettings = ExternalProcess.CmdLineProcessSettingsFactory.GetProcessSettings(decoderExecutableFile);

            processSettings.ArgumentList.Add(CmdLineDecoderOptions.Decode);
            processSettings.ArgumentList.Add(CmdLineDecoderOptions.WriteToSdtOut);

            processSettings.ArgumentList.Add(sourceFile.FullName);

            return processSettings;
        }
    }
}