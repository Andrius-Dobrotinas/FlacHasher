using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.FlacHash.Input.Flac
{
    public class CmdLineStreamDecoder
    {
        private readonly FileInfo decoderExecutableFile;

        public CmdLineStreamDecoder(FileInfo decoderExecutableFile)
        {
            this.decoderExecutableFile = decoderExecutableFile ?? throw new ArgumentNullException(nameof(decoderExecutableFile));
        }

        public Stream Read(Stream wavAudio)
        {
            var processSettings = GetProcessSettings(decoderExecutableFile);

            try
            {
                return ExternalProcess.ProcessRunner.RunAndReadOutput(processSettings, wavAudio);
            }
            catch (ExternalProcess.ExecutionException e)
            {
                var message = $"Failed to read the input file. FLAC process exited with error code {e.ExitCode}.\nFLAC error output: {e.ProcessErrorOutput}";

                throw new InputReadingException(message);
            }
        }

        private static ProcessStartInfo GetProcessSettings(FileInfo decoderExecutableFile)
        {
            var processSettings = ExternalProcess.CmdLineProcessSettingsFactory.GetProcessSettings(decoderExecutableFile);

            processSettings.ArgumentList.Add(CmdLineDecoderOptions.Decode);
            processSettings.ArgumentList.Add("-"); // read from stdin

            return processSettings;
        }
    }
}