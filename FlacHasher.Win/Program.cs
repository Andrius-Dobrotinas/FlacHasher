using Andy.FlacHash.Cmd;
using Andy.FlacHash.Win.IO;
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
                var services = BuildHasher(settings.Decoder);
                Application.Run(
                    new FormX(
                        services.Item1,
                        new InteractiveTextFileWriter(saveFileDialog),
                        new HashFaceValueFactory(hashRepresentationFormat),
                        services.Item2));
            }
        }

        private static Tuple<IMultipleFileHasher, FileReadProgressReporter> BuildHasher(FileInfo decoderFile)
        {
            var fileReadProgressReporter = new FileReadProgressReporter();
            var steamFactory = new ProgressReportingReadStreamFactory(fileReadProgressReporter);
            var decoder = new Input.Flac.CmdLineStreamDecoder(decoderFile);
            var reader = new DecodingFileReader(steamFactory, decoder);

            var hasher = new FileHasher(reader, new Crypto.Sha256HashComputer());

            return new Tuple<IMultipleFileHasher, FileReadProgressReporter>(
                new MultipleFileHasher(hasher),
                fileReadProgressReporter);
        }
    }
}