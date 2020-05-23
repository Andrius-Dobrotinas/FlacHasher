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

            CompressedSizeService compressionService = new CompressedSizeService(
                new CmdLineFlacEncoderFactory(flacExe));

            var serviz = new CompressionLevelInferrer(
                compressionService,
                minCompressionLevel,
                maxCompressionLevel);

            using (var dialog = BuildOpenFileDialog())
            {
                var dialogWrapper = new UI.FileOpenDialog(dialog);

                using (var form = new MainForm(
                    maxCompressionLevel,
                    defaultCompressionLevel,
                    serviz,
                    dialogWrapper))
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
    }
}