using Andy.FlacHash.Audio.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            uint maxCompressionLevel = 8;

            FileInfo flacExe = new FileInfo(@"C:\Program Files (x86)\FLAC Frontend\tools\flac.exe");

            WellIsIt wellIsIt = (sourceFile, compressionLevel) =>
            {
                var recoder = new CmdLineFlacRecoder(flacExe, compressionLevel);

                using (MemoryStream recodedAudio = recoder.Encode(sourceFile))
                {
                    return new Tuple<long, long>(sourceFile.Length, recodedAudio.Length);
                }
            };

            using (var dialog = BuildOpenFileDialog())
            {
                var dialogWrapper = new UI.FileOpenDialog(dialog);

                using (var form = new MainForm(
                    maxCompressionLevel,
                    6,
                    wellIsIt,
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