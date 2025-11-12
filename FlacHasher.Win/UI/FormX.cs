using Andy.FlacHash.Audio;
using Andy.FlacHash.Crypto;
using Andy.FlacHash.Hashfile.Read;
using Andy.FlacHash.Hashing;
using Andy.FlacHash.Verification;
using Andy.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public partial class FormX : Form
    {
        const string errorSeparator = ".......................";
        const int hashfileMaxSizeBytes = 512 * 1024;

        private readonly HasherFactory hasherFactory;
        private readonly InteractiveTextFileWriter hashFileWriter;
        private readonly FileSizeProgressBarAdapter progressReporter;

        private readonly InteractiveDirectoryGetter dirBrowser;
        private readonly InputFileResolver targetFileResolver;

        private readonly IHashFormatter hashFormatter;
        private readonly HashFileReader hashFileParser;
        private readonly HashVerifier hashVerifier;
        private readonly DecoderProfile[] decoderProfiles;
        private readonly int defaultAlgorithmIndex;
        private readonly OpenFileDialog openFileDialog_hashfile;
        private readonly OpenFileDialog openFileDialog_inputFiles;
        private readonly Settings settings;

        private NonBlockingHashComputation hasherService;
        Dictionary<HasherKey, NonBlockingHashComputation> hashers = new Dictionary<HasherKey, NonBlockingHashComputation>();
        Dictionary<string, FileInfo> DecoderExes = new Dictionary<string, FileInfo>();

        private bool finishedWithErrors;
        private bool freshOperation = false;
        private DirectoryInfo directory;
        private IFileListView fileList;
        private IDictionary<FileInfo, string> fileHashes;
        private Mode mode;

        private DecoderProfile DecoderProfile => (DecoderProfile)menu_decoderProfiles.SelectedItem;
        private FileExtensionsOption SelectedFileType => (FileExtensionsOption)menu_fileExtensions.SelectedItem;
        private Algorithm SelectedHashingAlgorithmProfile => (Algorithm)menu_hashingAlgorithm.SelectedItem;

        public FormX(
            HasherFactory hasherFactory,
            InteractiveTextFileWriter hashFileWriter,
            IDataReadEventSource fileReadEventSource,
            InteractiveDirectoryGetter dirBrowser,
            InputFileResolver targetFileResolver,
            IHashFormatter hashFormatter,
            HashFileReader hashFileParser,
            HashVerifier hashVerifier,
            DecoderProfile[] decoderProfiles,
            Algorithm[] hashingAlgorithmOptions,
            Settings settings,
            OpenFileDialog openFileDialog_hashfile,
            OpenFileDialog openFileDialog_inputFiles)
        {
            InitializeComponent();

            this.hashFileWriter = hashFileWriter;
            this.dirBrowser = dirBrowser;
            this.targetFileResolver = targetFileResolver;
            this.hashFormatter = hashFormatter;
            this.hashFileParser = hashFileParser;
            this.hashVerifier = hashVerifier;
            this.hasherFactory = hasherFactory;
            this.decoderProfiles = decoderProfiles;
            this.openFileDialog_hashfile = openFileDialog_hashfile;
            this.openFileDialog_inputFiles = openFileDialog_inputFiles;
            this.settings = settings;

            this.defaultAlgorithmIndex = Util.FindIndex(hashingAlgorithmOptions, x => x == settings.HashAlgorithm);

            this.progressReporter = new FileSizeProgressBarAdapter(progressBar);
            fileReadEventSource.BytesRead += (bytesRead) =>
            {
                this.Invoke(new Action(() => progressReporter.Increment(bytesRead)));
            };

            ResultListContextMenuSetup.WireUp(
                list_results,
                ctxMenu_results,
                () => WithTryCatch(SaveHashesHandler),
                (useConfiguredFormatting) => WithTryCatch(() => CopyHashesHandler(useConfiguredFormatting)));

            // List etc initialization
            menu_decoderProfiles.DisplayMember = nameof(DecoderProfile.Name);
            menu_decoderProfiles.Items.AddRange(decoderProfiles);
            menu_decoderProfiles.SelectedIndex = 0;

            menu_hashingAlgorithm.Items.AddRange(hashingAlgorithmOptions.Cast<object>().ToArray());
            menu_hashingAlgorithm.SelectedIndex = defaultAlgorithmIndex;

            menu_fileExtensions.DisplayMember = nameof(FileExtensionsOption.Name);
            var exts = decoderProfiles.Select(x => new FileExtensionsOption(x.Name, x.TargetFileExtensions)).ToArray();
            menu_fileExtensions.Items.AddRange(exts);
            menu_fileExtensions.SelectedIndex = 0;


            // Initial values
            btn_go.Enabled = false;

            // Wire handlers up AFTER setting initial values to avoid them getting fired before everything is ready
            menu_decoderProfiles.SelectedIndexChanged += decoderProfiles_SelectedIndexChanged;
            menu_hashingAlgorithm.SelectedIndexChanged += hashingProfiles_SelectedIndexChanged;
            menu_fileExtensions.SelectedIndexChanged += (object sender, EventArgs e) =>
            {
                RefreshHashingFilelist();
            };
        }

        private void FormX_Load(object sender, EventArgs e)
        {
            LoadFormSettings();

            // Triggers all kinds of handlers
            List_hashing_results_Resize(null, null);
            List_verification_results_Resize(null, null);
            SetMode(Mode.Hashing);
            ResetStatusMessages();

            // mode and decoder+algorithm have to be pre-set
            WithTryCatch(BuildHasherCached);
        }

        bool listResults_selectionReversal = false;
        private void List_results_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listResults_selectionReversal)
            {
                listResults_selectionReversal = false;
                return;
            }
            if (Form.MouseButtons == MouseButtons.Right)
            {
                listResults_selectionReversal = true;
                e.Item.Selected = !e.IsSelected;
            }
        }

        private void list_results_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (var i in list_results.Items.Cast<ListViewItem>())
                    i.Selected = true;
                return;
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                CopyHashesHandler(false);
                return;
            }
        }

        void ResetStatusMessages()
        {
            if (mode == Mode.Hashing)
                ResetLog("Select a directory to hash some files!", TextResources.F1Hint);
            else
                ResetLog("Select a hashfile to verify files", TextResources.F1Hint);
        }

        private async Task WithTryCatch_Operation(Func<Task> function)
        {
            try
            {
                await function();
            }
            catch (Exception ex)
            {
                ShowFatalOperationError(ex);
            }
        }

        private bool WithTryCatch(Action function)
        {
            try
            {
                function();
                return true;
            }
            catch (Exception ex)
            {
                ShowFatalError(ex);
                return false;
            }
        }

        private void List_hashing_results_Resize(object sender, EventArgs e)
        {
            var newWidth = list_results.Width / 2;
            columnHashResult_File.Width = newWidth;
            columnHashResult_Hash.Width = list_results.Width - newWidth;
        }

        private void List_verification_results_Resize(object sender, EventArgs e)
        {
            var newWidth = list_verification_results.Width * 0.8;

            col_results_verification_file.Width = (int)newWidth;
            col_results_verification_isMatch.Width = list_verification_results.Width - col_results_verification_file.Width;
        }

        private void BtnChooseDir_Click(object sender, EventArgs e)
        {
            WithTryCatch(ChooseHashingDir);
        }

        private void BtnChooseFiles_Click(object sender, EventArgs e)
        {
            WithTryCatch(ChooseHashingInputFiles);
        }

        private void BtnChooseHashfile_Click(object sender, EventArgs e)
        {
            WithTryCatch(ChooseHashVerificationFile);
        }

        private void ChooseHashingDir()
        {
            var selectedDir = dirBrowser.GetDirectory();
            if (selectedDir == null) return;
            ChooseHashingDir(selectedDir);
        }

        private void ChooseHashingDir(DirectoryInfo selectedDirectory)
        {
            directory = selectedDirectory;
            ResetLog($"Current directory: {directory.FullName}");

            SetMode(Mode.Hashing);
            SetHashingFileExtensionMenuAvailability();

            RefreshHashingFilelist();

            menu_fileExtensions.Focus();
        }

        void ChooseHashingInputFiles()
        {
            var selectedFiles = GetFilesFromUser(openFileDialog_inputFiles);
            if (selectedFiles == null) return;

            var inputFiles = selectedFiles.Select(x => new FileInfo(x)).ToArray();

            SetSelectedInputFiles(inputFiles);
        }

        void SetSelectedInputFiles(FileInfo[] inputFiles)
        {
            directory = null;
            ResetLog();

            SetMode(Mode.Hashing);
            SetHashingFileExtensionMenuAvailability();

            SetNewInputFiles(inputFiles);

            menu_decoderProfiles.Focus();
        }

        void ChooseHashVerificationFile()
        {
            var selectedFiles = GetFilesFromUser(openFileDialog_hashfile);
            if (selectedFiles == null) return;

            var hashfile = new FileInfo(selectedFiles.First());
            ChooseHashVerificationFile(hashfile);
        }
        void ChooseHashVerificationFile(FileInfo hashfile)
        {

            if (hashfile.Length > hashfileMaxSizeBytes && MessageBox.Show(
                        $"The selected hashfile (\"{hashfile.Name}\") is quite big ({hashfileMaxSizeBytes / 1024} kb), " +
                        $"which means it may contain some more serious (and unusable for this purposes) data. " +
                        $"Do you want to continue?",
                        "Hashfile too big?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning)
                    != DialogResult.Yes)
                return;

            directory = hashfile.Directory;

            var fileHashMap = hashFileParser.Read(hashfile);
            if (fileHashMap.IsPositionBased)
            {
                MessageBox.Show("Hashfile must contain file names (use the cmd-line tool to verify a plain no-names hashlist)", "Not so fast", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var fileExt = Util.DeDotFileExtension(hashfile.Extension);
            SetHashingAlgorigthm(fileExt);

            ResetLog($"Hashfile: {hashfile.FullName}", $"Working directory: {directory.FullName}");

            SetMode(Mode.Verification);

            LoadFilesFromHashmap(fileHashMap);

            menu_decoderProfiles.Focus();
        }

        void RefreshHashingFilelist()
        {
            if (!isHashingDirectorySelected())
                return;

            var files = targetFileResolver.FindFiles(directory, SelectedFileType.Extensions);
            SetNewInputFiles(files);
        }

        private void SetNewInputFiles(FileInfo[] files)
        {
            fileList.Reset(files);
            freshOperation = true;

            progressReporter.Reset(0);

            btn_go.Enabled = fileList.Any();

            SelectAppropriateDecoder();

            if (!fileList.Any())
                if (mode == Mode.Hashing)
                    LogMessage("The directory doesn't contain suitable files!");
                else
                    LogMessage("The hashfile doesn't contain any data!");
            else
                LogMessage("Press Go!");
        }

        void SetHashingAlgorigthm(string fileExtension)
        {
            var matchingAlgo = menu_hashingAlgorithm.Items.Cast<Algorithm>()
                .Select(x => new { Str = x.ToString(), Org = x })
                .FirstOrDefault(
                    x => x.Str.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));
            if (matchingAlgo != null)
                menu_hashingAlgorithm.SelectedItem = matchingAlgo.Org;
            else
                menu_hashingAlgorithm.SelectedIndex = defaultAlgorithmIndex;
        }

        bool ExtensionSaysHashfile(string fileExtensionWithDot)
        {
            var actualExtension = fileExtensionWithDot.Split('.').Skip(1).First().ToLowerInvariant();
            return menu_hashingAlgorithm.Items.Cast<Algorithm>()
                .Select(x => x.ToString().ToLowerInvariant())
                .Concat(new[] { "hash", "txt" })
                .Contains(actualExtension);
        }

        void SelectAppropriateDecoder()
        {
            if (!fileList.Any())
                return;

            var ext = Util.DeDotFileExtension(fileList.ItemKeys.FirstOrDefault().Extension);
            var match = decoderProfiles.FirstOrDefault(x => x.TargetFileExtensions.Contains(ext));
            if (match != null)
                menu_decoderProfiles.SelectedItem = match;
        }

        void SaveHashesHandler()
        {
            SaveHashes(list_results.BackingData.Where(x => x.Value?.HashString != null));
        }

        void SaveHashes(IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>> results)
        {
            var lines = results.Select(x => OutputFormatting.GetFormattedString(settings.OutputFormat, x.Value.HashString, x.Value.File));
            if (hashFileWriter.GetFileAndSave(lines) == true)
                LogMessage("Hashes saved!");
        }

        void CopyHashesHandler(bool useConfiguredFormatting)
        {
            var items = GetSelectedData(list_results);
            CopyHashes(items, useConfiguredFormatting);
        }

        void CopyHashes(IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>> results, bool useConfiguredFormatting)
        {
            var values = useConfiguredFormatting
                ? results.Select(x => OutputFormatting.GetFormattedString(settings.OutputFormat, x.Value?.HashString, x.Key))
                : results.Select(x => $"{x.Key.Name}\t{x.Value?.HashString}");

            Clipboard.SetText(string.Join(Environment.NewLine, values));
        }

        static IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>> GetSelectedData(FileHashResultList resultList)
        {
            var selectedItems = resultList.ListViewItems.Where(x => x.Selected);
            if (selectedItems.Any())
                return selectedItems.Select(x => new KeyValuePair<FileInfo, FileHashResultListItem>(x.Key, x.Data));
            else
                return resultList.BackingData;
        }

        private async void Btn_Go_Click(object sender, EventArgs e)
        {
            if (hasherService == null)
                if (WithTryCatch(BuildHasherOrThrow) == false)
                    return;
            await WithTryCatch_Operation(Go);
        }

        private async Task Go()
        {
            if (!hasherService.InProgress)
            {
                if (!freshOperation)
                    fileList.ResetData();

                switch (mode)
                {
                    case Mode.Hashing:
                        {
                            await ComputeHashes();
                            return;
                        }
                    case Mode.Verification:
                        {
                            await VerifyHashes();
                            return;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                OnOperationCancellation(); //TODO: make the hasher service invoke this when cancelled?
                hasherService.Cancel();
            }
        }

        private async Task VerifyHashes()
        {
            var expectedFiles = fileHashes.Select(x => x.Key);
            SetProgressBar(expectedFiles);
            await VerifyHashes(fileHashes);
        }

        private async Task VerifyHashes(IDictionary<FileInfo, string> expectedHashes)
        {
            var exist = expectedHashes.Where(x => x.Key.Exists).ToList();

            foreach (var file in expectedHashes.Except(exist))
            {
                // TODO: if hashfile position-based, replace file name with something more generic
                list_verification_results.SetData(file.Key, HashMatch.NotFound);
            }

            var files = exist.Select(x => x.Key);
            await hasherService.Start(files,
                (FileHashResult hashingResult) =>
                {
                    if (hashingResult.Exception == null)
                    {
                        var isMatch = hashVerifier.DoesMatch(expectedHashes, hashingResult.File, hashingResult.Hash);

                        list_verification_results.SetData(hashingResult.File, isMatch ? HashMatch.Match : HashMatch.NoMatch);
                    }
                    else
                    {
                        var result = (hashingResult.Exception is InputFileNotFoundException)
                            ? HashMatch.NotFound
                            : HashMatch.Error;
                        list_verification_results.SetData(hashingResult.File, result);

                        ReportExecutionError(hashingResult.Exception, hashingResult.File);
                    }
                });
        }

        private void SetProgressBar(IEnumerable<FileInfo> files)
        {
            // Name-based verification includes files even if they don't exist just so they can be reported in the correct order
            long totalSize = files.Select(file => file.Exists ? file.Length : 0).Sum();
            progressReporter.Reset(totalSize);
        }

        void SetSelectorElementAccesibility(bool isEnabled)
        {
            btn_chooseDir.Enabled = isEnabled;
            btn_chooseFiles.Enabled = isEnabled;
            btn_openHashfile.Enabled = isEnabled;
            menu_decoderProfiles.Enabled = isEnabled;
            menu_hashingAlgorithm.Enabled = isEnabled;
            SetHashingFileExtensionMenuAvailability(!isEnabled);
            ctxMenu_results.Enabled = isEnabled;
        }

        void SetHashingFileExtensionMenuAvailability(bool forceDisable = false)
        {
            /* It should only be available when choosing a Directory for hashing.
             * Not when choosing specific files, not when doing verification.
             * It's important to react to in-progress transitions too */
            menu_fileExtensions.Enabled = (forceDisable == true)
                ? false
                : isHashingDirectorySelected();
        }

        bool isHashingDirectorySelected()
        {
            return mode == Mode.Hashing && directory != null;
        }

        private void OnOperationCancellation()
        {
            btn_go.Enabled = false;
            btn_go.Text = "Stopping...";
        }

        private void OnOperationStateTransition(bool inProgress)
        {
            if (inProgress)
            {
                finishedWithErrors = false;
                freshOperation = false;
                ResetLog("Working...");
            }

            SetSelectorElementAccesibility(!inProgress);

            btn_go.Text = inProgress ? "Stop" : "Go!"; //todo: put these into a resource file
        }

        private void OnOperationFinished(bool cancelled)
        {
            if (cancelled)
            {
                btn_go.Enabled = true;
                progressReporter.Reset(0);

                LogMessage("Canceled");
            }
            else
            {
                progressReporter.SetToMax();

                if (finishedWithErrors)
                    LogMessage("Finished with errors ^^");
                else
                    LogMessage("Finished");
            }
        }

        /// <summary>
        /// Gets invoked when an operation totally fails (doesn't move on to the next file)
        /// </summary>
        private void OnFailure(Exception e)
        {
            btn_go.Enabled = true;
            progressReporter.Reset(0);

            LogMessage("Failed");
            ShowFatalOperationError(e);
        }

        private async Task ComputeHashes()
        {
            var files = fileList.ItemKeys.ToArray();

            SetProgressBar(files);
            await hasherService.Start(files, UpdateUIWithHashingResult);
        }

        private void UpdateUIWithHashingResult(FileHashResult result)
        {
            if (result.Exception != null)
            {
                ReportExecutionError(result.Exception, result.File);
                return;
            }

            this.list_results.SetData(
                result.File,
                new FileHashResultListItem
                {
                    File = result.File,
                    HashString = hashFormatter.GetString(result.Hash)
                });
        }

        private void ReportExecutionError(Exception exception, FileInfo file)
        {
            finishedWithErrors = true;
            LogMessage($"Error processing file: {file.Name}", exception.Message);
        }

        void LogMessage(params string[] message)
        {
            if (!string.IsNullOrEmpty(txtStatus.Text))
            {
                txtStatus.AppendText(errorSeparator);
                txtStatus.AppendText(Environment.NewLine);
            }

            foreach (var line in message)
            {
                txtStatus.AppendText(line);
                txtStatus.AppendText(Environment.NewLine);
            }
        }

        void WriteLine(string message)
        {
            txtStatus.AppendText(message);
            txtStatus.AppendText(Environment.NewLine);
        }

        void ResetLog(params string[] message)
        {
            txtStatus.Text = null;
            if (message != null && message.Any())
                LogMessage(message);
        }

        void ShowFatalOperationError(Exception e)
        {
            LogMessage($"Error processing file(s)", e.Message);
            MessageBox.Show($"Operation failed. See the error log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void ShowFatalError(Exception e)
        {
            ResetLog(e.Message);
            if (e is ConfigurationException)
                LogMessage(TextResources.F1Hint);

            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SetMode(Mode mode)
        {
            this.mode = mode;
            fileList = mode == Mode.Hashing
                ? (IFileListView)list_results
                : list_verification_results;

            this.list_results.Visible = mode == Mode.Hashing;
            this.list_verification_results.Visible = mode == Mode.Verification;

            SetHashingFileExtensionMenuAvailability();
        }

        void LoadFilesFromHashmap(FileHashMap fileHashMap)
        {
            // Storing fileHashes separately from fileList is good for repeating the same op on the same input: fileHashes doesn't have to be re-read
            fileHashes = ReadFilesFromHashmap(fileHashMap);

            SetNewInputFiles(fileHashes.Select(x => x.Key).ToArray());
        }

        IDictionary<FileInfo, string> ReadFilesFromHashmap(FileHashMap fileHashMap)
        {
            var filesInCurrentDir = fileHashMap.Hashes
                .Select(
                    x => new FileInfo(
                        Path.Combine(directory.FullName, x.Key)))
                .ToArray();
            return HashEntryMatching.MatchFilesToHashes(fileHashMap, filesInCurrentDir);
        }

        private void decoderProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            WithTryCatch(BuildHasherCached);
        }

        private void hashingProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            WithTryCatch(BuildHasherCached);
        }

        private void BuildHasherOrThrow()
        {
            if (!DecoderExes.ContainsKey(DecoderProfile.Decoder))
            {
                FileInfo decoderExe = Audio.AudioDecoder.ResolveDecoderOrThrow(DecoderProfile.Decoder);
                DecoderExes.Add(DecoderProfile.Decoder, decoderExe);
            }

            this.hasherService = HashComputationServiceFactory.Build(
                hasherFactory.BuildDecoder(DecoderExes[DecoderProfile.Decoder], DecoderProfile.DecoderParameters, SelectedHashingAlgorithmProfile),
                this,
                OnOperationFinished,
                OnFailure,
                OnOperationStateTransition);
        }

        private void BuildHasherCached()
        {
            var key = new HasherKey(DecoderProfile.Name, SelectedHashingAlgorithmProfile);

            hashers.TryGetValue(key, out hasherService);

            if (hasherService != null)
                return;

            BuildHasherOrThrow();
            hashers.Add(key, this.hasherService);
        }

        static string[] GetFilesFromUser(OpenFileDialog box)
        {
            var result = box.ShowDialog();
            if (result != DialogResult.OK) return null;

            return box.FileNames;
        }

        private void list_results_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
        }

        private void list_results_DragDrop(object sender, DragEventArgs e)
        {
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (paths.Any(File.Exists))
            {
                var files = paths.Where(File.Exists).Select(x => new FileInfo(x)).ToArray();

                // If there's a hashfile, treat it as a verification op. Don't care about the rest of the files
                var first = files.First();
                if (ExtensionSaysHashfile(first.Extension))
                    ChooseHashVerificationFile(first);
                else
                    SetSelectedInputFiles(files);
            }
            else if (paths.Any(Directory.Exists))
            {
                var directory = new DirectoryInfo(paths.First());
                ChooseHashingDir(directory);
            }
        }

        private void FormX_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveFormSettings();
        }

        void SaveFormSettings()
        {
            Properties.Default.WindowHeight = this.Height;
            Properties.Default.WindowWidth = this.Width;
            Properties.Default.WindowState = (int)this.WindowState;
            Properties.Default.DecoderProfile = this.menu_decoderProfiles.SelectedIndex;
            Properties.Default.HashingAlgo = this.menu_hashingAlgorithm.SelectedIndex;
            Properties.Default.Save();
        }

        void LoadFormSettings()
        {
            if (Properties.Default.WindowHeight != 0 && Properties.Default.WindowWidth != 0)
            {
                this.Height = Properties.Default.WindowHeight;
                this.Width = Properties.Default.WindowWidth;
            }

            if (this.menu_decoderProfiles.Items.Count > Properties.Default.DecoderProfile)
                this.menu_decoderProfiles.SelectedIndex = Properties.Default.DecoderProfile;
            if (this.menu_hashingAlgorithm.Items.Count > Properties.Default.HashingAlgo)
                this.menu_hashingAlgorithm.SelectedIndex = Properties.Default.HashingAlgo;

            this.WindowState = (FormWindowState)Properties.Default.WindowState;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                ResetLog();
                WriteLine(HelpUtil.GetHelpText().ToString());
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void toolStripButtonSettings_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsForm(settings))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    Properties.Default.ApplicationSettings = form.Result; 
                    Properties.Default.Save();
                    System.Windows.Forms.Application.Restart();
                }
            }
        }

        struct HasherKey
        {
            public string DecoderProfile { get; }
            public Algorithm HashAlgorightm { get; }

            public HasherKey(string DecoderProfile, Algorithm HashAlgorightm)
            {
                this.DecoderProfile = DecoderProfile;
                this.HashAlgorightm = HashAlgorightm;
            }
        }
    }
}