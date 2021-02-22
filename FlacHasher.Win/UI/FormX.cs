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
        private readonly HashCalculationService hasherService;
        private readonly InteractiveTextFileWriter hashFileWriter;
        private readonly FileSizeProgressBarAdapter progressReporter;
        private readonly InteractiveDirectoryFileGetter directoryFileGetter;

        public FormX(
            HashCalculationServiceFactory hashCalculationServiceFactory,
            InteractiveTextFileWriter hashFileWriter,
            IDisplayValueProducer<FileHashResult> displayValueProducer,
            IO.IFileReadEventSource fileReadEventSource,
            InteractiveDirectoryFileGetter directoryFileGetter)
        {
            InitializeComponent();

            this.list_results.DisplayValueProducer = displayValueProducer;

            this.hashFileWriter = hashFileWriter;
            this.directoryFileGetter = directoryFileGetter;

            this.hasherService = hashCalculationServiceFactory.Build(
                this,
                OnCalcFinished,
                OnCalcStateChanged);

            progressReporter = new FileSizeProgressBarAdapter(progressBar);

            fileReadEventSource.BytesRead += (bytesRead) => {
                this.Invoke(new Action(() => progressReporter.Increment(bytesRead)));
            };

            ResultListContextMenuSetup.WireUp(list_results, ctxMenu_results, SaveHashes);

            this.label_Status.Text = "Select a directory";
        }

        private void BtnChooseDir_Click(object sender, EventArgs e)
        {
            var files = directoryFileGetter.GetFiles();
            if (files == null) return;

            if (files.Any() == false)
                label_Status.Text = "The selected directory doesn't contain files suitable files";
            else
                label_Status.Text = @"Press ""Go""";

            SetNewInputFiles(files);
        }

        private void SetNewInputFiles(FileInfo[] files)
        {
            list_files.ReplaceItems(files);
            list_results.ClearList();
            progressReporter.Reset(0);
            btn_go.Enabled = files.Any();
        }

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
                hasherService.Start(files, UpdateUIWithCalcResult);
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
    }
}