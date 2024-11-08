using Andy.FlacHash.Crypto;
using Andy.FlacHash.Hashfile.Read;
using Andy.FlacHash.Verification;
using Andy.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win
{
    static class Program
    {
        const string settingsFileName = "settings.cfg";

        [STAThread]
        static void Main()
        {
            Settings settings;
            DecoderProfile[] decoderProfiles;
            try
            {
                var settingsFile = new FileInfo(settingsFileName);
                var result = SettingsFile.GetSettings(settingsFile);
                settings = result.Item1;
                decoderProfiles = result.Item2;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failure reading a settings file. {e.Message}");
                return;
            }

            var algorithms = GetAllEnumValues<Algorithm>().ToArray();

            var hashfileExtensions = FileExtension.PrefixWithDot(settings.HashfileExtensions);
            var fileSearch = new FileSearch(settings.FileLookupIncludeHidden);

            System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            var algosString = BuildFileFilter(algorithms);

            using (var openFileDialog_inputFiles = Build_OpenInputfilesDialog(
                PrepSupportedFileExtensions(decoderProfiles)))
            using (var openFileDialog_hashfile = Build_OpenHashfileDialog(algosString))
            using (var saveHashesToFileDialog = Build_SaveHashesToFileDialog(algosString))
            using (var directoryResolver = Build_InteractiveDirectoryResolverGetter())
            {
                var fileReadProgressReporter = new FileReadProgressReporter();
                var processRunner = new ExternalProcess.ProcessRunner(
                    settings.ProcessTimeoutSec,
                    settings.ProcessExitTimeoutMs,
                    settings.ProcessStartWaitMs,
                    showProcessWindowWithStdErrOutput: settings.ShowProcessWindowWithOutput);

                var hasherFactory = new HasherFactory(processRunner, fileReadProgressReporter, settings);
                var hashFormatter = new PlainLowercaseHashFormatter();

                System.Windows.Forms.Application.Run(
                    new UI.FormX(
                        hasherFactory,
                        new UI.InteractiveTextFileWriter(saveHashesToFileDialog),
                        fileReadProgressReporter,
                        directoryResolver,
                        new InputFileResolver(
                            hashfileExtensions,
                            fileSearch),
                        hashFormatter,
                        HashFileReader.Default.BuildHashfileReader(settings.HashfileEntrySeparator),
                        new HashVerifier(hashFormatter),
                        decoderProfiles,
                        algorithms,
                        settings,
                        openFileDialog_hashfile,
                        openFileDialog_inputFiles));
            }
        }

        private static UI.InteractiveDirectoryGetter Build_InteractiveDirectoryResolverGetter()
        {
            var dirBrowser = new FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            return new UI.InteractiveDirectoryGetter(dirBrowser);
        }

        private static OpenFileDialog Build_OpenInputfilesDialog(string filter)
        {
            return new OpenFileDialog
            {
                CheckPathExists = true,
                DereferenceLinks = true,
                Filter = filter,
                Title = "Select files",
                Multiselect = true
            };
        }

        private static OpenFileDialog Build_OpenHashfileDialog(string filterSpecificFiles)
        {
            return new OpenFileDialog
            {
                CheckPathExists = true,
                DereferenceLinks = true,
                Filter = $"Any file type|*.*|Hash|*.hash|{filterSpecificFiles}|Text files|*.txt",
                Title = "Open a Hash File",
                Multiselect = false
            };
        }

        private static SaveFileDialog Build_SaveHashesToFileDialog(string filterSpecificFiles)
        {
            return new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                Filter = $"Hash|*.hash|Text files|*.txt|{filterSpecificFiles}|Any|*.*",
                Title = "Save As"
            };
        }

        static IEnumerable<TEnum> GetAllEnumValues<TEnum>()
        {
            foreach (var value in Enum.GetValues(typeof(TEnum)))
                yield return (TEnum)value;
        }

        static string PrepSupportedFileExtensions(IEnumerable<DecoderProfile> decoderProfiles)
        {
            var filters = decoderProfiles.Select(x =>
            {
                var extensionString = string.Join(';', x.TargetFileExtensions.Select(x => $"*.{x}"));
                return $"{x.Name}|{extensionString}";
            }).ToList();
            var filterString = string.Join('|', filters);

            return $"{filterString}|Other files|*.*";
        }

        static string BuildFileFilter(IEnumerable<Algorithm> algorithms)
            => string.Join('|', algorithms.Select(x => $"{x}|*.{x}"));
    }
}