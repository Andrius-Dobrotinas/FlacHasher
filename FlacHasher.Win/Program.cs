using Andy.Cmd.Parameter;
using Andy.FlacHash.Cmd;
using Andy.FlacHash.Hashing;
using Andy.FlacHash.Hashing.Verification;
using Andy.FlacHash.IO.Progress;
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

        [STAThread]
        static void Main()
        {
            Settings settings;
            try
            {
                var settingsFile = new FileInfo(settingsFileName);
                var settingsFileParams = SettingsFile.ReadIniFile(settingsFile)
                    .ToDictionary(x => x.Key, x => new[] { x.Value });

                settings = ParameterReader.Build().GetParameters<Settings>(settingsFileParams);
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
                FileInfo decoderExe = Andy.FlacHash.Cmd.Program.ResolveDecoderOrThrow(settings);

                var (hasher, progressReporter) = BuildHasher(decoderExe, settings);
                var hashFormatter = new PlainLowercaseHashFormatter();

                var fileExtension = settings.TargetFileExtension
                    ?? (AudioDecoder.IsFlac(decoderExe)
                        ? Audio.Flac.FormatMetadata.FileExtension
                        : throw new ConfigurationException("Configure file extension for the specified decoder"));

                Application.Run(
                    new UI.FormX(
                        new UI.HashCalculationServiceFactory(
                            hasher),
                        new UI.InteractiveTextFileWriter(saveHashesToFileDialog),
                        progressReporter,
                        directoryResolver,
                        new InputFileResolver(
                            hashfileExtensions,
                            fileSearch),
                        hashFormatter,
                        Cmd.Verification.BuildHashfileReader(settings.HashfileEntrySeparator),
                        new HashVerifier(hashFormatter),
                        fileExtension));
            }
        }

        private static (IReportingMultiFileHasher, FileReadProgressReporter) BuildHasher(FileInfo decoderExe, Settings settings)
        {
            var fileReadProgressReporter = new FileReadProgressReporter();

            var decoderParams = AudioDecoder.GetDefaultDecoderParametersIfEmpty(settings.DecoderParameters, decoderExe);
            var decoder = AudioDecoder.Build(
                decoderExe,
                new ExternalProcess.ProcessRunner(
                    settings.ProcessTimeoutSec,
                    settings.ProcessExitTimeoutMs,
                    settings.ProcessStartWaitMs,
                    showProcessWindowWithStdErrOutput: settings.ShowProcessWindowWithOutput),
                decoderParams,
                fileReadProgressReporter);

            var hasher = new FileHasher(
                decoder,
                new Hashing.Crypto.Hasher(settings.HashAlgorithm));
            var cancellableHasher = new ReportingMultiFileHasher(
                new MultiFileHasher(
                    hasher, 
                    continueOnError: true,
                    decoder is Audio.StdInputStreamAudioFileDecoder ? null : fileReadProgressReporter));

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