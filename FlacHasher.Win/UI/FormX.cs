﻿using Andy.FlacHash.Verification;
using Andy.FlacHash.Verification.Source;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public partial class FormX : Form
    {
        private readonly NonBlockingHashComputation hasherService;
        private readonly InteractiveTextFileWriter hashFileWriter;
        private readonly FileSizeProgressBarAdapter progressReporter;

        private readonly InteractiveDirectoryGetter dirBrowser;
        private readonly TargetFileResolver targetFileResolver;

        private readonly IHashFormatter hashFormatter;
        private readonly HashFileReader hashFileParser;
        private readonly HashVerifier hashVerifier;

        private bool finishedWithErrors;

        public FormX(
            HashCalculationServiceFactory hashCalculationServiceFactory,
            InteractiveTextFileWriter hashFileWriter,
            IO.IFileReadEventSource fileReadEventSource,
            InteractiveDirectoryGetter dirBrowser,
            TargetFileResolver targetFileResolver,
            IHashFormatter hashFormatter,
            HashFileReader hashFileParser,
            HashVerifier hashVerifier)
        {
            InitializeComponent();

            this.hashFileWriter = hashFileWriter;
            this.dirBrowser = dirBrowser;
            this.targetFileResolver = targetFileResolver;
            this.hashFormatter = hashFormatter;
            this.hashFileParser = hashFileParser;
            this.hashVerifier = hashVerifier;

            this.hasherService = hashCalculationServiceFactory.Build(
                this,
                OnCalcFinished,
                OnFailure,
                OnCalcStateChanged);

            this.progressReporter = new FileSizeProgressBarAdapter(progressBar);

            fileReadEventSource.BytesRead += (bytesRead) => {
                this.Invoke(new Action(() => progressReporter.Increment(bytesRead)));
            };

            ResultListContextMenuSetup.WireUp(list_results, ctxMenu_results, SaveHashes);

            this.label_Status.Text = "Select a directory";
            this.btn_go.Enabled = false;

            this.mode_Calc.Checked = true;
            SetMode(Mode.Calculation);

            this.list_files.Initialize();
            this.list_hashFiles.Initialize();
            this.list_results.Initialize();

            this.list_verification_results.View = View.Details;

            list_verification_results.SmallImageList = imgList_verification;

            list_verification_results.Resize += List_verification_results_Resize;
            List_verification_results_Resize(null, null);
        }
        private void List_verification_results_Resize(object sender, EventArgs e)
        {
            var newWidth = list_verification_results.Width - col_results_verification_isMatch.Width;

            col_results_verification_file.Width = (int)(newWidth * .95);
        }

        private void BtnChooseDir_Click(object sender, EventArgs e)
        {
            var directory = dirBrowser.GetDirectory();
            if (directory == null) return;

            var (files, hashFiles) = targetFileResolver.GetFiles(directory);

            if (files.Any() == false)
                label_Status.Text = "The selected directory doesn't contain suitable files";
            else
                label_Status.Text = @"Press ""Go""";

            SetNewInputFiles(files, hashFiles);
        }

        private void SetNewInputFiles(FileInfo[] files, FileInfo[] hashFiles)
        {
            list_files.ReplaceItems(files);

            if (hashFiles != null)
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
            btn_go.Enabled = list_files.Any() && (mode == Mode.Calculation || list_hashFiles.Any());
        }

        private void SaveHashes(IEnumerable<FileHashResultListItem> results)
        {
            var hashes = results.Select(x => x.HashString);

            if (hashFileWriter.GetFileAndSave(hashes) == true)
                MessageBox.Show("Hashes saved!");
        }

        private async void Btn_Go_Click(object sender, EventArgs e)
        {
            if (!hasherService.InProgress)
            {
                var files = GetFiles();

                switch (mode)
                {
                    case Mode.Calculation:
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
                throw new FileNotFoundException($"Hash file doesn't exist: {hashFile.FullName}");

            return hashFileParser.Read(hashFile);
        }

        private async Task VerifyHashes(IList<FileInfo> files, FileHashMap expectedHashes)
        {
            var (existingFileHashes, missingFileHashes) = HashEntryMatching.MatchFilesToHashes(expectedHashes, files);
            var existingFileHashDictionary = existingFileHashes.ToDictionary(x => x.Key, x => x.Value);

            BeforeCalc(existingFileHashDictionary.Keys);
            await VerifyHashes(existingFileHashDictionary);

            if (missingFileHashes != null)
                foreach (var item in missingFileHashes)
                    list_verification_results.Add(item.Key, HashMatch.NotFound);
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
                        var result = (calcResult.Exception is FileNotFoundException)
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

            // Name-based verification includes files even if they don't exist just so they can be reported in the correct order
            long totalSize = files.Select(file => file.Exists ? file.Length : 0).Sum();
            progressReporter.Reset(totalSize);
        }

        private void OnCalcCancellation()
        {
            btn_go.Enabled = false;
            btn_go.Text = "Stopping...";
        }

        private void OnCalcStateChanged(bool inProgress)
        {
            btn_go.Text = inProgress ? "Stop" : "Go!"; //todo: put these into a resource file
            this.label_Status.Text = "Working...";
        }

        private void OnCalcFinished(bool cancelled)
        {
            if (cancelled)
            {
                btn_go.Enabled = true;
                progressReporter.Reset(0);

                label_Status.Text = "Canceled";
            }
            else
            {
                if (finishedWithErrors)
                    progressReporter.SetToMax();

                label_Status.Text = "Finished";
            }
        }

        private void OnFailure(Exception e)
        {
            btn_go.Enabled = true;
            progressReporter.Reset(0);

            label_Status.Text = "Failed";

            MessageBox.Show($"Error processing file(s): {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            Task.Run(() => MessageBox.Show($"Error processing file {file.Name}: {exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
        }

        public enum Mode
        {
            Calculation,
            Verification
        }

        private Mode mode;

        private void mode_Calc_CheckedChanged(object sender, EventArgs e)
        {
            SetMode(Mode.Calculation);
        }

        private void mode_Verify_CheckedChanged(object sender, EventArgs e)
        {
            SetMode(Mode.Verification);
        }

        private void SetMode(Mode mode)
        {
            this.mode = mode;

            this.list_results.Visible = mode == Mode.Calculation;
            this.list_verification_results.Visible = mode == Mode.Verification;

            this.list_files.Size = new System.Drawing.Size(660, 139);


            Set_Go_Button_State();
        }
    }
}