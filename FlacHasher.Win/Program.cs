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
        const string supportedFileExtension = "flac";
        const string hashFileExtension = "hash";
        const int processExitTimeoutMsDefault = 1000;
        const int processStartWaitMsDefault = 100;
        const int processTimeoutSecDefault = 180;
        const bool showProcessWindowWithOutput = false; // todo: read this from the settings file
        const bool continueOnErrorDefault = true;

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
                var (hasher, progressReporter) = BuildHasher(settings);
                var hashFormatter = new PlainLowercaseHashFormatter();

                Application.Run(
                    new UI.FormX(
                        new UI.HashCalculationServiceFactory(
                            hasher),
                        new UI.InteractiveTextFileWriter(saveHashesToFileDialog),
                        progressReporter,
                        directoryResolver,
                        new InputFileResolver(
                            supportedFileExtension,
                            hashFileExtension),
                        hashFormatter,
                        Cmd.Verification.BuildHashfileReader(),
                        new HashVerifier(hashFormatter)));
            }
        }

        private static (IReportingMultiFileHasher, FileReadProgressReporter) BuildHasher(Settings settings)
        {
            var fileReadProgressReporter = new FileReadProgressReporter();
            var steamFactory = new IO.ProgressReportingReadStreamFactory(fileReadProgressReporter);
            var decoder = new IO.Audio.Flac.CmdLine.StreamDecoder(
                settings.Decoder,
                new ExternalProcess.ProcessRunner(
                    settings.ProcessTimeoutSec ?? processTimeoutSecDefault,
                    settings.ProcessExitTimeoutMs ?? processExitTimeoutMsDefault,
                    settings.ProcessStartWaitMs ?? processStartWaitMsDefault,
                    showProcessWindowWithOutput));
            var reader = new IO.Audio.FileStreamDecoder(steamFactory, decoder);

            var hasher = new FileHasher(reader, new Crypto.Sha256HashComputer());
            var cancellableHasher = new ReportingMultiFileHasher(
                new MultiFileHasher(hasher, !settings.FailOnError ?? continueOnErrorDefault));

            return (cancellableHasher, fileReadProgressReporter);
        }

        private static UI.InteractiveDirectoryGetter Build_InteractiveDirectoryResolverGetter()
        {
            var dirBrowser = new FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            return new UI.InteractiveDirectoryGetter(dirBrowser);
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