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

        public FormX(
            HashCalculationServiceFactory hashCalculationServiceFactory,
            InteractiveTextFileWriter hashFileWriter,
            IDisplayValueProducer<FileHashResult> displayValueProducer,
            IO.IFileReadEventSource fileReadEventSource,
            InteractiveDirectoryFileGetter dirBrowser,
            TargetFileResolver targetFileResolver)
        {
            InitializeComponent();

            this.list_results.DisplayValueProducer = displayValueProducer;

            this.hashFileWriter = hashFileWriter;
            this.dirBrowser = dirBrowser;
            this.targetFileResolver = targetFileResolver;

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

            this.list_verification_results.View = View.Details;
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
            this.hashFile = hashFile;
            list_results.ClearList();
            progressReporter.Reset(0);
            
            Set_Go_Button_State();
        }

        private void Set_Go_Button_State()
        {
            var isVerificationPossible = hashFile != null;

            btn_go.Enabled = list_files.Any() && (mode == Mode.Calculation || isVerificationPossible);
        }

        private FileInfo hashFile;

        private void SaveHashes(IEnumerable<ListItem<FileHashResult>> results)
        {
            var hashes = results.Select(x => x.DisplayValue);

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
                            var targetHashes = File.ReadAllLines(hashFile.FullName);
                            int i = 0;
                            hasherService.Start(files,
                                (FileHashResult result) =>
                                {
                                    var hash = BitConverter.ToString(result.Hash)
                                        .Replace("-", "").ToLowerInvariant();
                                    var isMatch = string.Equals(targetHashes[i], hash, StringComparison.OrdinalIgnoreCase);

                                    var item = new ListViewItem
                                    {
                                        Text = result.File.Name,
                                    };

                                    item.SubItems.Add(isMatch.ToString());

                                    this.list_verification_results.Items.Add(item);

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
            return this.list_files.GetItems().ToList();
        }

        private void BeforeCalc(IEnumerable<FileInfo> files)
        {
            this.list_results.ClearList();

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
            this.list_results.AddItem(result);
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

            Set_Go_Button_State();
        }
    }
}