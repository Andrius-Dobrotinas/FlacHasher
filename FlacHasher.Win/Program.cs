using Andy.Cmd.Parameter;
using Andy.FlacHash.Cmd;
using Andy.FlacHash.Hashing;
using Andy.FlacHash.Hashing.Verification;
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
                var settingsFileParams = SettingsProvider.GetSettingsDictionary(settingsFile)
                    .ToDictionary(x => x.Key, x => new[] { x.Value });

                settings = ParameterReader.GetParameters<Settings>(settingsFileParams);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failure reading a settings file. {e.Message}");
                return;
            }

            var hashfileExtensions = Cmd.Verification.Settings.GetHashFileExtensions(settings.HashfileExtensions);
            var fileSearch = new FileSearch(settings.FileLookupIncludeHidden);

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
                            hashfileExtensions,
                            fileSearch),
                        hashFormatter,
                        Cmd.Verification.BuildHashfileReader(settings.HashfileEntrySeparator),
                        new HashVerifier(hashFormatter)));
            }
        }

        private static (IReportingMultiFileHasher, FileReadProgressReporter) BuildHasher(ApplicationSettings settings)
        {
            var fileReadProgressReporter = new FileReadProgressReporter();
            var steamFactory = new IO.ProgressReportingReadStreamFactory(fileReadProgressReporter);
            var decoder = new Audio.StreamDecoder(
                new FileInfo(settings.Decoder),
                new ExternalProcess.ProcessRunner(
                    settings.ProcessTimeoutSec ?? processTimeoutSecDefault,
                    settings.ProcessExitTimeoutMs ?? processExitTimeoutMsDefault,
                    settings.ProcessStartWaitMs ?? processStartWaitMsDefault,
                    showProcessWindowWithOutput),
                Audio.Flac.CmdLine.Parameters.Decode.Stream);
            var reader = new Audio.AudioFileDecoder(steamFactory, decoder);

            var hasher = new FileHasher(
                reader,
                new Hashing.Crypto.HashComputer(ExecutionParameterResolver.ParseHashAlgorithmOrGetDefault(settings.HashAlgorithm)));
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