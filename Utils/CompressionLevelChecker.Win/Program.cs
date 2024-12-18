using Andy.FlacHash.Audio;
using Andy.FlacHash.Audio.Flac;
using Andy.FlacHash.CompressionLevel.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    static class Program
    {
        private const int defaultCompressionLevel = (int)Application.Audio.Flac.CompressionLevel.Highest;
        private const int processExitTimeoutMs = 1000;
        private const int processStartDelayMs = 100;
        const int processTimeoutSec = 180;

        // todo: these values must be stored somewhere else
        private const int maxCompressionLevel = (int)Application.Audio.Flac.CompressionLevel.Highest;
        private const int minCompressionLevel = (int)Application.Audio.Flac.CompressionLevel.Lowest;

        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // todo: read this from something like a settings file, or something
            FileInfo flacExe = new FileInfo(@"C:\Program Files (x86)\FLAC Frontend\tools\flac.exe");

            CompressionLevelService mainService = BuildComponents(flacExe);

            using (var dialog = BuildOpenFileDialog())
            {
                var fileOpenDialog = new UI.FileOpenDialog(dialog);

                using (var form = new MainForm(
                    maxCompressionLevel,
                    minCompressionLevel,
                    defaultCompressionLevel,
                    mainService,
                    fileOpenDialog))
                {
                    System.Windows.Forms.Application.Run(form);
                };
            }
        }

        private static OpenFileDialog BuildOpenFileDialog()
        {
            return new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                Filter = "FLAC|*.flac;*.fla",
                Title = "Select a file"
            };
        }

        private static CompressionLevelService BuildComponents(FileInfo flacExe)
        {
            var processRunner = new ExternalProcess.ProcessRunner(processTimeoutSec, processExitTimeoutMs, processStartDelayMs, showProcessWindowWithStdErrOutput: true);

            IAudioFileEncoder encoder_MetadataPreserved = new FileRecoder(flacExe, processRunner);

            IAudioFileEncoder encoder_MetadataDiscarded = new AudioFileEncoder(
                new AudioFileDecoder(flacExe, processRunner, Application.Audio.Flac.Parameters.Decode.File),
                new StreamEncoder(flacExe, processRunner));

            IFileInfoSizeGetter fileSize = new FileInfoSizeGetter();

            var service_MetadataPreserved = new CompressionLevelInferrer(
                new CompressedSizeService(encoder_MetadataPreserved),
                fileSize,
                minCompressionLevel,
                maxCompressionLevel);

            var service_MetadataDiscarded = new CompressionLevelInferrer(
                new CompressedSizeService(encoder_MetadataDiscarded),
                fileSize,
                minCompressionLevel,
                maxCompressionLevel);

            return new CompressionLevelService(service_MetadataPreserved, service_MetadataDiscarded);
        }
    }
}