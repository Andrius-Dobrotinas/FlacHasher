using Andy.FlacHash.Verification;
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
        private readonly NonBlockingHashCalculationService hasherService;
        private readonly InteractiveTextFileWriter hashFileWriter;
        private readonly FileSizeProgressBarAdapter progressReporter;

        private readonly InteractiveDirectoryFileGetter dirBrowser;
        private readonly TargetFileResolver targetFileResolver;

        private readonly IHashFormatter hashFormatter;
        private readonly HashFileParser hashFileParser;
        private readonly HashVerifier hashVerifier;

        private readonly VerificationResultsListWrapper verification_results;

        public FormX(
            HashCalculationServiceFactory hashCalculationServiceFactory,
            InteractiveTextFileWriter hashFileWriter,
            IO.IFileReadEventSource fileReadEventSource,
            InteractiveDirectoryFileGetter dirBrowser,
            TargetFileResolver targetFileResolver,
            IHashFormatter hashFormatter,
            HashFileParser hashFileParser,
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
            this.verification_results = new VerificationResultsListWrapper(list_verification_results);
        }

        private void BtnChooseDir_Click(object sender, EventArgs e)
        {
            var directory = dirBrowser.GetDirectory();
            if (directory == null) return;

            var result = targetFileResolver.GetFiles(directory);

            var files = result.Value.Item1;
            var hashFile = result.Value.Item2;

            if (files.Any() == false)
                label_Status.Text = "The selected directory doesn't contain files suitable files";
            else
                label_Status.Text = @"Press ""Go""";

            SetNewInputFiles(files, hashFile);
        }

        private void SetNewInputFiles(FileInfo[] files, FileInfo hashFile)
        {
            list_files.ReplaceItems(files);

            // TODO: I want it to list all available hash files, just in case there's more than one
            if (hashFile != null)
                list_hashFiles.ReplaceItems(hashFile);
            else
                list_hashFiles.Items.Clear();

            list_results.ClearList();
            list_verification_results.Items.Clear();

            progressReporter.Reset(0);
            
            Set_Go_Button_State();
        }

        private void Set_Go_Button_State()
        {
            var isVerificationPossible = list_hashFiles.Any();

            btn_go.Enabled = list_files.Any() && (mode == Mode.Calculation || isVerificationPossible);
        }

        private void SaveHashes(IEnumerable<FileHashResultListItem> results)
        {
            var hashes = results.Select(x => x.HashString);

            if (hashFileWriter.GetFileAndSave(hashes) == true)
                MessageBox.Show("Hashes saved!");
        }

        private void Btn_Go_Click(object sender, EventArgs e)
        {
            if (!hasherService.InProgress)
            {
                var files = GetFiles();
                BeforeCalc(files);

                switch (mode)
                {
                    case Mode.Calculation:
                        {
                            hasherService.Start(files, UpdateUIWithCalcResult);
                            return;
                        }
                    case Mode.Verification:
                        {
                            var expectedHashes = GetExpectedHashes();

                            int i = 0;

                            hasherService.Start(files,
                                (FileHashResult result) =>
                                {
                                    var isMatch = hashVerifier.DoesMatch(expectedHashes, result);

                                    verification_results.Add(result.File, isMatch);

                                    i++;
                                });
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

        private IEnumerable<FileInfo> GetFiles()
        {
            return list_files.GetItems().ToList();
        }
        
        private KeyValuePair<string, string>[] GetExpectedHashes()
        {
            var hashFile = list_hashFiles.GetItems().First();

            if (hashFile.Exists == false)
                throw new FileNotFoundException($"Hash file doesn't exist: {hashFile.FullName}");

            var lines = File.ReadAllLines(hashFile.FullName);

            var expectedHashes = hashFileParser
                .Parse(lines)
                .ToArray();

            if (expectedHashes.Any(x => string.IsNullOrEmpty(x.Key)))
                throw new Exception("Some entries in the hash file don't specify a file name");
            else if (expectedHashes.Select(x => x.Key).Distinct().Count() != expectedHashes.Length)
                throw new Exception("Some file names are repeated in the file");

            return expectedHashes;
        }

        private void BeforeCalc(IEnumerable<FileInfo> files)
        {
            list_results.ClearList();
            list_verification_results.Items.Clear();

            long totalSize = files.Select(file => file.Length).Sum();
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
                label_Status.Text = "Finished";
            }
        }

        private void UpdateUIWithCalcResult(FileHashResult result)
        {
            this.list_results.AddItem(
                new FileHashResultListItem
                {
                    Value = result,
                    HashString = hashFormatter.GetString(result.Hash)
                });

            this.Text = result.File.Name;
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