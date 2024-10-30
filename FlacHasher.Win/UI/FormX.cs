﻿using Andy.FlacHash.Audio;
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
        private readonly HasherFactory hasherFactory;
        private readonly InteractiveTextFileWriter hashFileWriter;
        private readonly FileSizeProgressBarAdapter progressReporter;

        private readonly InteractiveDirectoryGetter dirBrowser;
        private readonly InputFileResolver targetFileResolver;

        private readonly IHashFormatter hashFormatter;
        private readonly HashFileReader hashFileParser;
        private readonly HashVerifier hashVerifier;

        private NonBlockingHashComputation hasherService;

        private bool finishedWithErrors;
        private DirectoryInfo directory;
        private DecoderProfile DecoderProfile => (DecoderProfile)menu_decoderProfiles.SelectedItem;
        private AlgorithmOption HashingAlgorithmProfile => (AlgorithmOption)menu_hashingAlgorithm.SelectedItem;

        const string newline = "\r\n";
        const string errorSeparator = "==========================";

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
            AlgorithmOption[] hashingAlgorithmOptions,
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

            this.progressReporter = new FileSizeProgressBarAdapter(progressBar);

            ResultListContextMenuSetup.WireUp(list_results, ctxMenu_results, (results) => WithTryCatch(() => SaveHashes(results)));

            // List etc initialization
            menu_decoderProfiles.DisplayMember = nameof(DecoderProfile.Name);
            menu_decoderProfiles.Items.AddRange(decoderProfiles);

            menu_hashingAlgorithm.Items.AddRange(hashingAlgorithmOptions);
            menu_hashingAlgorithm.DisplayMember = nameof(AlgorithmOption.Name);

            list_verification_results.View = View.Details;
            list_verification_results.SmallImageList = imgList_verification;

            this.list_files.Initialize();
            this.list_hashFiles.Initialize();
            this.list_results.Initialize();
            
            // Initial values
            menu_decoderProfiles.SelectedIndex = 0;
            menu_hashingAlgorithm.SelectedIndex = Util.FindIndex(hashingAlgorithmOptions, x => x.Value == settings.HashAlgorithm);
            this.btn_go.Enabled = false;
            this.mode_Calc.Checked = true;

            // Wire handlers up AFTER setting initial values to avoid them getting fired before everything is ready
            fileReadEventSource.BytesRead += (bytesRead) =>
            {
                this.Invoke(new Action(() => progressReporter.Increment(bytesRead)));
            };
            menu_decoderProfiles.SelectedIndexChanged += decoderProfiles_SelectedIndexChanged;
            menu_hashingAlgorithm.SelectedIndexChanged += hashingProfiles_SelectedIndexChanged;
            list_verification_results.Resize += List_verification_results_Resize;

            // Triggers all kinds of handlers
            List_verification_results_Resize(null, null);
            SetMode(Mode.Hashing);

            BuildHasher();
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

        private void List_verification_results_Resize(object sender, EventArgs e)
        {
            var newWidth = list_verification_results.Width - col_results_verification_isMatch.Width;

            col_results_verification_file.Width = (int)(newWidth * .95);
        }

        private void BtnChooseDir_Click(object sender, EventArgs e)
        {
            WithTryCatch(ChooseDir);
        }

        private void ChooseDir()
        {
            directory = dirBrowser.GetDirectory();
            if (directory == null) return;

            RefreshFilelist();
        }

        void RefreshFilelist()
        {
            if (directory == null)
                return;

            var (files, hashFiles) = targetFileResolver.FindFiles(directory, DecoderProfile.TargetFileExtension);

            if (files.Any() == false)
                ResetLog("The selected directory doesn't contain suitable files");
            else
                ResetLog(@"Press ""Go""");

            SetNewInputFiles(files, hashFiles);
        }

        private void SetNewInputFiles(FileInfo[] files, FileInfo[] hashFiles)
        {
            list_files.ReplaceItems(files);

            if (hashFiles.Any())
            {
                list_hashFiles.ReplaceItems(hashFiles);
                list_hashFiles.SelectedIndex = 0;
            }
            else
                list_hashFiles.Items.Clear();

            list_results.ClearList();
            list_verification_results.Items.Clear();

            progressReporter.Reset(0);

            Set_Go_Button_State();
        }

        private void Set_Go_Button_State()
        {
            btn_go.Enabled = list_files.Any() && (mode == Mode.Hashing || list_hashFiles.Any());

            if (!list_files.Any())
                ResetLog("Select a directory that contains files");
            else if (mode == Mode.Verification && !list_hashFiles.Any())
                ResetLog("Select a directory that contains a hashfile");
            else
                ResetLog("Select Operation and Press Go");
        }

        private void SaveHashes(IEnumerable<FileHashResultListItem> results)
        {
            var hashes = results.Select(x => x.HashString);

            if (hashFileWriter.GetFileAndSave(hashes) == true)
                MessageBox.Show("Hashes saved!");
        }

        private async void Btn_Go_Click(object sender, EventArgs e)
        {
            await WithTryCatch(Go);
        }

        private async Task Go()
        {
            if (!hasherService.InProgress)
            {
                var files = GetFiles();

                switch (mode)
                {
                    case Mode.Hashing:
                        {
                            ComputeHashes(files);
                            return;
                        }
                    case Mode.Verification:
                        {
                            var expectedHashes = GetExpectedHashes();
                            await VerifyHashes(files, expectedHashes);
                            return;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                OnCalcCancellation(); //TODO: make the hasher service invoke this when cancelled?
                hasherService.Cancel();
            }
        }

        private IList<FileInfo> GetFiles()
        {
            return list_files.GetItems().ToList();
        }

        private FileHashMap GetExpectedHashes()
        {
            var hashFile = list_hashFiles.GetSelectedItem();

            if (hashFile.Exists == false)
                throw new Exception($"Hash file doesn't exist: {hashFile.FullName}");

            return hashFileParser.Read(hashFile);
        }

        private async Task VerifyHashes(IList<FileInfo> files, FileHashMap expectedHashes)
        {
            var fileHashes = HashEntryMatching.MatchFilesToHashes(expectedHashes, files);

            BeforeCalc(fileHashes.Keys);
            await VerifyHashes(fileHashes);
        }

        private async Task VerifyHashes(IDictionary<FileInfo, string> expectedHashes)
        {
            var files = expectedHashes.Keys;

            await hasherService.Start(files,
                (FileHashResult calcResult) =>
                {
                    if (calcResult.Exception == null)
                    {
                        var isMatch = hashVerifier.DoesMatch(expectedHashes, calcResult.File, calcResult.Hash);

                        list_verification_results.Add(calcResult.File, isMatch ? HashMatch.True : HashMatch.False);
                    }
                    else
                    {
                        var result = (calcResult.Exception is InputFileNotFoundException)
                            ? HashMatch.NotFound
                            : HashMatch.Error;
                        list_verification_results.Add(calcResult.File, result);
                        ReportExecutionError(calcResult.Exception, calcResult.File);
                    }
                });
        }

        private void BeforeCalc(IEnumerable<FileInfo> files)
        {
            finishedWithErrors = false;

            list_results.ClearList();
            list_verification_results.Items.Clear();
            txtStatus.Clear();

            // Name-based verification includes files even if they don't exist just so they can be reported in the correct order
            long totalSize = files.Select(file => file.Exists ? file.Length : 0).Sum();
            progressReporter.Reset(totalSize);

            ResetLog("Working...");
            tabs_Results.SelectTab(tabStatus);
        }

        private void OnCalcCancellation()
        {
            btn_go.Enabled = false;
            btn_go.Text = "Stopping...";
        }

        private void OnCalcStateChanged(bool inProgress)
        {
            grpModes.Enabled = !inProgress;

            btn_go.Text = inProgress ? "Stop" : "Go!"; //todo: put these into a resource file
        }

        private void OnCalcFinished(bool cancelled)
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

                // Verification reflects errors, the user can go back to the log
                if (mode == Mode.Verification || !finishedWithErrors)
                    tabs_Results.SelectTab(tabResults);
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

        private void ComputeHashes(IEnumerable<FileInfo> files)
        {
            BeforeCalc(files);
            hasherService.Start(files, UpdateUIWithCalcResult);
        }

        private void UpdateUIWithCalcResult(FileHashResult result)
        {
            if (result.Exception != null)
            {
                ReportExecutionError(result.Exception, result.File);
                return;
            }

            this.list_results.AddItem(
                new FileHashResultListItem
                {
                    Value = result,
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
            txtStatus.AppendText(errorSeparator);
            txtStatus.AppendText(newline);

            foreach (var line in message)
            {
                txtStatus.AppendText(line);
                txtStatus.AppendText(newline);
            }
        }

        void ResetLog(string message)
        {
            txtStatus.Text = message;
            txtStatus.AppendText(newline);
        }

        void ShowFatalError(Exception e)
        {
            LogMessage($"Error processing file(s)", e.Message);
            MessageBox.Show($"Operation failed. See the error log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private Mode mode;

        private void mode_Calc_CheckedChanged(object sender, EventArgs e)
        {
            SetMode(Mode.Hashing);
        }

        private void mode_Verify_CheckedChanged(object sender, EventArgs e)
        {
            SetMode(Mode.Verification);
        }

        private void SetMode(Mode mode)
        {
            this.mode = mode;

            this.list_results.Visible = mode == Mode.Hashing;
            this.list_verification_results.Visible = mode == Mode.Verification;

            Set_Go_Button_State();
        }

        private void decoderProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildHasher();

            RefreshFilelist();
        }

        private void hashingProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildHasher();
        }

        private void BuildHasher()
        {
            this.hasherService = HashComputationServiceFactory.Build(
                hasherFactory.BuildDecoder(DecoderProfile.Decoder, DecoderProfile.DecoderParameters, HashingAlgorithmProfile.Value),
                this,
                OnCalcFinished,
                OnFailure,
                OnCalcStateChanged);
        }
    }
}