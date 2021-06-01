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
        private readonly HashFileReader hashFileParser;
        private readonly HashVerifier hashVerifier;

        public FormX(
            HashCalculationServiceFactory hashCalculationServiceFactory,
            InteractiveTextFileWriter hashFileWriter,
            IO.IFileReadEventSource fileReadEventSource,
            InteractiveDirectoryFileGetter dirBrowser,
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

            var result = targetFileResolver.GetFiles(directory);

            var files = result.Value.Item1;
            var hashFiles = result.Value.Item2;

            if (files.Any() == false)
                label_Status.Text = "The selected directory doesn't contain files suitable files";
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
            var isVerificationPossible = list_hashFiles.Any();

            btn_go.Enabled = list_files.Any() && (mode == Mode.Calculation || isVerificationPossible);
        }

        private void SaveHashes(IEnumerable<FileHashResultListItem> results)
        {
            var hashes = results.Select(x => x.HashString);

            if (hashFileWriter.GetFileAndSave(hashes) == true)
                MessageBox.Show("Hashes saved!");
        }

        private (IList<KeyValuePair<string, string>>, IList<KeyValuePair<string, string>>, IList<FileInfo>) GetHashDataPositionBased(IList<KeyValuePair<string, string>> expectedHashes, IEnumerable<FileInfo> files)
        {
            var filesTargetedByTheHashes = files.Take(expectedHashes.Count).ToList();

            var entriesWithFilesMissing = filesTargetedByTheHashes.Count < expectedHashes.Count
                ? expectedHashes
                    .Skip(filesTargetedByTheHashes.Count)
                    //.Select(x => new KeyValuePair<string, string>("File Missing", x.Value)
                    .ToArray()
                : new KeyValuePair<string, string>[0];

            return (expectedHashes, entriesWithFilesMissing, filesTargetedByTheHashes);
        }

        private (IList<KeyValuePair<string, string>>, IList<KeyValuePair<string, string>>, IList<FileInfo>) GetHashData(IList<KeyValuePair<string, string>> expectedHashes, IEnumerable<FileInfo> files)
        {
            var filesByNames = files.ToDictionary(x => x.Name, x => x);

            var expectedHashToTargetFileMap = expectedHashes
                .ToDictionary(
                    x => x,
                    x => filesByNames.GetValueOrDefault(x.Key));

            var hashesWithExistingFiles = expectedHashToTargetFileMap
                .Where(x => x.Value != null);

            // Just take files that are included in the hash map
            var filesTargetedByTheHashes = hashesWithExistingFiles
                .Select(x => x.Value)
                .ToArray();

            var existingHashes = hashesWithExistingFiles
                .Select(x => x.Key)
                .ToArray();

            var missingFiles = expectedHashToTargetFileMap
                .Where(x => x.Value == null)
                .Select(x => x.Key)
                .ToArray();

            return (existingHashes, missingFiles, filesTargetedByTheHashes);
        }

        private void Btn_Go_Click(object sender, EventArgs e)
        {
            if (!hasherService.InProgress)
            {
                var files = GetFiles();

                switch (mode)
                {
                    case Mode.Calculation:
                        {
                            BeforeCalc(files);

                            hasherService.Start(files, UpdateUIWithCalcResult);
                            return;
                        }
                    case Mode.Verification:
                        {
                            var expectedHashes = GetExpectedHashes();

                            IList<KeyValuePair<string, string>> existingFileHashes;
                            IEnumerable<KeyValuePair<string, string>> missingFileHashes;
                            IList<FileInfo> filesTargetedByTheHashes;

                            (existingFileHashes, missingFileHashes, filesTargetedByTheHashes) = expectedHashes.IsPositionBased
                                    ? GetHashDataPositionBased(expectedHashes.Hashes, files)
                                    : GetHashData(expectedHashes.Hashes, files);

                            var actualFileCount = filesTargetedByTheHashes.Count;

                            int i = 0;

                            BeforeCalc(filesTargetedByTheHashes);

                            hasherService.Start(filesTargetedByTheHashes,
                                (FileHashResult calcResult) =>
                                {
                                    var isMatch = hashVerifier.DoesMatch(existingFileHashes, i, calcResult.Hash);

                                    list_verification_results.Add(calcResult.File, isMatch);

                                    i++;

                                    // When finished, add all missing files to the list
                                    if (i == actualFileCount)
                                    {
                                        foreach (var item in missingFileHashes)
                                            list_verification_results.Add(
                                                new FileInfo(
                                                    expectedHashes.IsPositionBased
                                                        ? "File Missing"
                                                        : item.Key),
                                                false);
                                    }
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