using Andy.Cmd.Parameter;
using Andy.FlacHash.Application.Audio;
using Andy.FlacHash.Audio;
using Andy.FlacHash.Hashfile.Read;
using Andy.FlacHash.Hashing;
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

            var hashfileExtensions = FileExtension.PrefixWithDot(settings.HashfileExtensions);
            var fileSearch = new FileSearch(settings.FileLookupIncludeHidden);

            System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            using (var saveHashesToFileDialog = Build_SaveHashesToFileDialog())
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
                        decoderProfiles));
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