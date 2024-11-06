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
        const string newline = "\r\n";
        const string errorSeparator = "==========================";

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

        private bool finishedWithErrors;
        private bool freshOperation = false;
        private DirectoryInfo directory;
        private IFileListView fileList;
        private IDictionary<FileInfo, string> fileHashes;
        private Mode mode;

        private DecoderProfile DecoderProfile => (DecoderProfile)menu_decoderProfiles.SelectedItem;
        private FileExtensionsOption SelectedFileType => (FileExtensionsOption)menu_fileExtensions.SelectedItem;
        private Algorithm HashingAlgorithmProfile => (Algorithm)menu_hashingAlgorithm.SelectedItem;

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
            Settings settings)
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
            this.settings = settings;

            this.defaultAlgorithmIndex = Util.FindIndex(hashingAlgorithmOptions, x => x == settings.HashAlgorithm);

            this.progressReporter = new FileSizeProgressBarAdapter(progressBar);
            
            openFileDialog_hashfile = new OpenFileDialog
            {
                CheckPathExists = true,
                DereferenceLinks = true,
                Filter = PrepSupportedHashfileExtensions(hashingAlgorithmOptions),
                Title = "Open a Hash File",
                Multiselect = false
            };
            openFileDialog_inputFiles = new OpenFileDialog
            {
                CheckPathExists = true,
                DereferenceLinks = true,
                Filter = PrepSupportedFileExtensions(decoderProfiles),
                Title = "Select files",
                Multiselect = true
            };

            ResultListContextMenuSetup.WireUp(
                list_results, 
                ctxMenu_results, 
                (results) => WithTryCatch(() => SaveHashes(results)),
                (results) => WithTryCatch(() => CopyHashes(results)));

            // List etc initialization
            menu_decoderProfiles.DisplayMember = nameof(DecoderProfile.Name);
            menu_decoderProfiles.Items.AddRange(decoderProfiles);

            menu_hashingAlgorithm.Items.AddRange(hashingAlgorithmOptions.Cast<object>().ToArray());

            menu_fileExtensions.DisplayMember = nameof(FileExtensionsOption.Name);

            var exts = decoderProfiles.Select(x => new FileExtensionsOption(x.Name, x.TargetFileExtensions))
                .ToArray();
            menu_fileExtensions.Items.AddRange(exts);
            menu_fileExtensions.SelectedIndex = 0;
            menu_fileExtensions.SelectedIndexChanged += (object sender, EventArgs e) =>
            {
                RefreshHashingFilelist();
            };

            list_verification_results.View = View.Details;
            list_verification_results.SmallImageList = imgList_verification;

            // Initial values
            menu_decoderProfiles.SelectedIndex = 0;
            menu_hashingAlgorithm.SelectedIndex = defaultAlgorithmIndex;
            this.btn_go.Enabled = false;

            // Wire handlers up AFTER setting initial values to avoid them getting fired before everything is ready
            fileReadEventSource.BytesRead += (bytesRead) =>
            {
                this.Invoke(new Action(() => progressReporter.Increment(bytesRead)));
            };
            menu_decoderProfiles.SelectedIndexChanged += decoderProfiles_SelectedIndexChanged;
            menu_hashingAlgorithm.SelectedIndexChanged += hashingProfiles_SelectedIndexChanged;

            list_results.Resize += List_hashing_results_Resize;
            list_verification_results.Resize += List_verification_results_Resize;

            // Triggers all kinds of handlers
            List_hashing_results_Resize(null, null);
            List_verification_results_Resize(null, null);
            SetMode(Mode.Hashing);

            BuildHasherCached();
            ResetStatusMessages();
        }

        void ResetStatusMessages()
        {
            if (mode == Mode.Hashing)
                ResetLog("Select a directory to hash some files!");
            else
                ResetLog("Select a hashfile to verify files");
        }

        private async Task WithTryCatch(Func<Task> function)
        {
            try
            {
                await function();
            }
            catch (Exception ex)
            {
                ShowFatalError(ex);
            }
        }

        private void WithTryCatch(Action function)
        {
            try
            {
                function();
            }
            catch (Exception ex)
            {
                ShowFatalError(ex);
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
            directory = dirBrowser.GetDirectory();
            if (directory == null) return;

            ResetLog($"Current directory: {directory.FullName}");

            SetMode(Mode.Hashing);
            SetHashingFileExtensionMenuAvailability();

            RefreshHashingFilelist();
        }

        void ChooseHashingInputFiles()
        {
            var selectedFiles = GetFilesFromUser(openFileDialog_inputFiles);
            if (selectedFiles == null) return;

            var inputFiles = selectedFiles.Select(x => new FileInfo(x)).ToArray();
            directory = null;
            ResetLog();

            SetMode(Mode.Hashing);
            SetHashingFileExtensionMenuAvailability();

            SetNewInputFiles(inputFiles);
        }

        void ChooseHashVerificationFile()
        {
            var selectedFiles = GetFilesFromUser(openFileDialog_hashfile);
            if (selectedFiles == null) return;

            var hashfile = new FileInfo(selectedFiles.First());
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
                .Select(x => x.ToString())
                .FirstOrDefault(
                    x => x.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));
            if (matchingAlgo != null)
                menu_hashingAlgorithm.SelectedItem = matchingAlgo;
            else
                menu_hashingAlgorithm.SelectedIndex = defaultAlgorithmIndex;
        }

        void SelectAppropriateDecoder()
        {
            if (!fileList.Any())
                return;

            var ext = Util.DeDotFileExtension(fileList.FirstOrDefault().Extension);
            var match = decoderProfiles.FirstOrDefault(x => x.TargetFileExtensions.Contains(ext));
            if (match != null)
                menu_decoderProfiles.SelectedItem = match;
        }

        private void SaveHashes(IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>> results)
        {
            var hashes = results.Where(x => x.Value?.HashString != null);

            var lines = hashes.Select(x => OutputFormatting.GetFormattedString(settings.OutputFormat, x.Value.HashString, x.Value.File));
            if (hashFileWriter.GetFileAndSave(lines) == true)
                LogMessage("Hashes saved!");
        }

        private void CopyHashes(IEnumerable<KeyValuePair<FileInfo, FileHashResultListItem>> results)
        {
            var values = results.Select(x => $"{x.Key.Name}\t{x.Value?.HashString}");

            Clipboard.SetText(string.Join(Environment.NewLine, values));
        }

        private async void Btn_Go_Click(object sender, EventArgs e)
        {
            await WithTryCatch(Go);
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

                        list_verification_results.SetData(hashingResult.File, isMatch ? HashMatch.True : HashMatch.False);
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
            ShowFatalError(e);
        }

        private async Task ComputeHashes()
        {
            var files = fileList.ToArray();

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

            this.Text = result.File.Name;
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
                txtStatus.AppendText(newline);
            }

            foreach (var line in message)
            {
                txtStatus.AppendText(line);
                txtStatus.AppendText(newline);
            }
        }

        void ResetLog(params string[] message)
        {
            txtStatus.Text = null;
            if (message != null && message.Any())
                LogMessage(message);
        }

        void ShowFatalError(Exception e)
        {
            LogMessage($"Error processing file(s)", e.Message);
            MessageBox.Show($"Operation failed. See the error log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            BuildHasherCached();
        }

        private void hashingProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildHasherCached();
        }

        private void BuildHasher()
        {
            this.hasherService = HashComputationServiceFactory.Build(
                hasherFactory.BuildDecoder(DecoderProfile.Decoder, DecoderProfile.DecoderParameters, HashingAlgorithmProfile),
                this,
                OnOperationFinished,
                OnFailure,
                OnOperationStateTransition);
        }

        private void BuildHasherCached()
        {
            var key = new HasherKey(DecoderProfile.Name, HashingAlgorithmProfile);

            hashers.TryGetValue(key, out hasherService);

            if (hasherService != null)
                return;

            BuildHasher();
            hashers.Add(key, this.hasherService);
        }

        static string[] GetFilesFromUser(OpenFileDialog box)
        {
            var result = box.ShowDialog();
            if (result != DialogResult.OK) return null;

            return box.FileNames;
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

        static string PrepSupportedHashfileExtensions(IEnumerable<Algorithm> algorithms)
        {
            var algosString = string.Join('|', algorithms.Select(x => $"{x}|*.{x}"));
            return $"Any file type|*.*|Hash|*.hash|{algosString}|Text files|*.txt";
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