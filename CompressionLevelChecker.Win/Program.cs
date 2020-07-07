using Andy.FlacHash.IO.Audio;
using Andy.FlacHash.IO.Audio.Flac.CmdLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    static class Program
    {
        private const uint defaultCompressionLevel = 8;

        // todo: these values must be stored somewhere else
        private const uint maxCompressionLevel = 8;
        private const uint minCompressionLevel = 0;

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // todo: read this from something like a settings file, or something
            FileInfo flacExe = new FileInfo(@"C:\Program Files (x86)\FLAC Frontend\tools\flac.exe");

            CompressionLevelService mainService = BuildComponents(flacExe);

            using (var dialog = BuildOpenFileDialog())
            {
                var fileOpenDialog = new UI.FileOpenDialog(dialog);

                using (var form = new MainForm(
                    maxCompressionLevel,
                    defaultCompressionLevel,
                    mainService,
                    fileOpenDialog))
                {
                    Application.Run(form);
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
            var processRunner = new ExternalProcess.ProcessRunner();

            IAudioFileEncoder encoder_MetadataPreserved = new CmdLineFlacRecoder(flacExe, processRunner);

            IAudioFileEncoder encoder_MetadataDiscarded = new AudioFileEncoder(
                new CmdLineFileDecoder(flacExe, processRunner),
                new CmdLineFlacEncoder(flacExe, processRunner));

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