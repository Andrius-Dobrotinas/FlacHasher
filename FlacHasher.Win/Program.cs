using Andy.FlacHash.Cmd;
using Andy.FlacHash.Verification;
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
        const string supportedFileExtension = ".flac";
        const string hashFileExtension = ".hash";
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
            using (var directoryResolver = Build_InteractiveDirectoryResolverGetter())
            {
                var (hasher, progressReporter) = BuildHasher(settings.Decoder);
                var hashFormatter = new HashFormatter();

                Application.Run(
                    new UI.FormX(
                        new UI.HashCalculationServiceFactory(
                            hasher,
                            new ProgressReportingOperationRunner()),
                        new InteractiveTextFileWriter(saveHashesToFileDialog),
                        progressReporter,
                        directoryResolver,
                        new TargetFileResolver(
                            supportedFileExtension,
                            hashFileExtension),
                        hashFormatter,
                        new ValidatingHashFileParser(
                            new HashFileParser(
                                new HashEntryParser())),
                        new HashVerifier(hashFormatter)));
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

        private static UI.InteractiveDirectoryFileGetter Build_InteractiveDirectoryResolverGetter()
        {
            var dirBrowser = new FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            return new UI.InteractiveDirectoryFileGetter(dirBrowser);
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