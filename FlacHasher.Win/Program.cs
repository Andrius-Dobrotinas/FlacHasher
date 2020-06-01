using Andy.FlacHash.Cmd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    static class Program
    {
        const string settingsFileName = "settings.cfg";
        const string hashRepresentationFormat = "{hash}";

        [STAThread]
        static void Main()
        {
            Settings settings;
            try
            {
                var settingsFile = new System.IO.FileInfo(settingsFileName);
                settings = SettingsProvider.GetSettings(settingsFile);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failure reading a settings file. {e.Message}");
                return;
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                Filter = "TEXT|*.txt|ANY|*.*",
                Title = "Save As"
            })
            {
                Application.Run(
                    new FormX(
                        settings.Decoder,
                        new HashWriter(saveFileDialog),
                        new HashFaceValueFactory(hashRepresentationFormat)));
            }
        }

        public static IEnumerable<FileHashResult> DoIt(FileInfo decoderFile, IEnumerable<FileInfo> inputFiles)
        {
            var decoder = new Input.Flac.CmdLineDecoder(decoderFile);
            var hasher = new FileHasher(decoder, new Crypto.Sha256HashComputer());
            var multiHasher = new MultipleFileHasher(hasher);

            return multiHasher.ComputeHashes(inputFiles);
        }
    }
}
