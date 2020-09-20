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
        const string supportedFileExtensions = "*.flac";
        const int processTimeoutSec = 300; // todo: read this from the settings file

        [STAThread]
        static void Main()
        {
            Settings settings;
            try
            {
                var settingsFile = new FileInfo(settingsFileName);
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

            using (var saveHashesToFileDialog = Build_SaveHashesToFileDialog())
            using (var sourceFileGetter = Build_DirectoryFileGetter(supportedFileExtensions))
            {
                var (hasher, progressReporter) = BuildHasher(settings.Decoder);
                Application.Run(
                    new UI.FormX(
                        new UI.HashCalculationServiceFactory(
                            hasher,
                            new ActionOnNonUiThreadRunner()),
                        new InteractiveTextFileWriter(saveHashesToFileDialog),
                        new UI.HashDisplayValueFactory(hashRepresentationFormat),
                        progressReporter,
                        sourceFileGetter));
            }
        }

        private static (IReportingMultipleFileHasher, FileReadProgressReporter) BuildHasher(FileInfo decoderFile)
        {
            var fileReadProgressReporter = new FileReadProgressReporter();
            var steamFactory = new IO.ProgressReportingReadStreamFactory(fileReadProgressReporter);
            var decoder = new IO.Audio.Flac.CmdLine.StreamDecoder(
                decoderFile,
                new ExternalProcess.ProcessRunner(processTimeoutSec));
            var reader = new IO.Audio.DecodingFileReader(steamFactory, decoder);

            var hasher = new FileHasher(reader, new Crypto.Sha256HashComputer());
            var cancellableHasher = new ReportingMultipleFileHasher(
                new MultipleFileHasher(hasher));

            return (cancellableHasher, fileReadProgressReporter);
        }

        private static UI.InteractiveDirectoryFileGetter Build_DirectoryFileGetter(string sourceFileFilter)
        {
            var dirBrowser = new FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };
            
            return new UI.InteractiveDirectoryFileGetter(dirBrowser, sourceFileFilter);
        }

        private static SaveFileDialog Build_SaveHashesToFileDialog()
        {
            return new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                Filter = "TEXT|*.txt|ANY|*.*",
                Title = "Save As"
            };
        }
    }
}