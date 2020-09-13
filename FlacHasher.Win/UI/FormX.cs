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
        private readonly HashCalcOnSeparateThreadService hasherService;
        private readonly InteractiveTextFileWriter hashFileWriter;
        private readonly FileSizeProgressBarAdapter progressReporter;
        private readonly InteractiveDirectoryFileGetter directoryFileGetter;
        private readonly CancellableActionRunner cancellableActionRunner = new CancellableActionRunner();

        public FormX(
            HashCalcOnSeparateThreadService hasherService,
            InteractiveTextFileWriter hashFileWriter,
            IDisplayValueProducer<FileHashResult> displayValueProducer,
            IO.IFileReadEventSource fileReadEventSource,
            InteractiveDirectoryFileGetter directoryFileGetter)
        {
            InitializeComponent();

            this.list_results.DisplayValueProducer = displayValueProducer;

            this.hasherService = hasherService;
            this.hashFileWriter = hashFileWriter;
            this.directoryFileGetter = directoryFileGetter;

            cancellableActionRunner.StateChanged += OnCalcStateChanged;
            cancellableActionRunner.Finished += OnCalcFinished;

            hasherService.OnHashResultAvailable += UpdateUIWithResult;
            hasherService.OnFinished += cancellableActionRunner.OnFinished;
            hasherService.UiUpdateContext = this;

            progressReporter = new FileSizeProgressBarAdapter(progressBar);

            fileReadEventSource.BytesRead += (bytesRead) => {
                this.Invoke(new Action(() => progressReporter.Increment(bytesRead)));
            };

            ResultListContextMenuSetup.WireUp(list_results, ctxMenu_results, SaveHashes);
        }

        private void BtnChooseDir_Click(object sender, EventArgs e)
        {
            var files = directoryFileGetter.GetFiles();
            if (files == null) return;

            list_files.ReplaceItems(files);
        }

        private void SaveHashes(IEnumerable<ListItem<FileHashResult>> results)
        {
            var hashes = results.Select(x => x.DisplayValue);

            if (hashFileWriter.GetFileAndSave(hashes) == true)
                MessageBox.Show("Hashes saved!");
        }

        private void Btn_Go_Click(object sender, EventArgs e)
        {
            if (!cancellableActionRunner.InProgress)
                cancellableActionRunner.Start(StartCalc);
            else
            {
                OnCancellation(); // todo this
                cancellableActionRunner.Cancel();
            }
        }

        private void StartCalc(CancellationToken cancellationToken)
        {
            var files = this.list_files.GetItems().ToList();

            this.list_results.ClearList();

            long totalSize = files.Select(file => file.Length).Sum();
            progressReporter.Reset(totalSize);

            hasherService.CalculateHashes(files, cancellationToken);
        }

        private void OnCancellation()
        {
            btn_go.Enabled = false;
            btn_go.Text = "Stopping...";
        }

        private void OnCalcStateChanged(bool inProgress)
        {
            btn_go.Text = inProgress ? "Stop" : "Go!"; //todo: put these into a resource file
        }

        private void OnCalcFinished(bool wasCancelled)
        {
            if (wasCancelled)
            {
                btn_go.Enabled = true;
                progressReporter.Reset(0);
            }
        }

        private void UpdateUIWithResult(FileHashResult result)
        {
            this.list_results.AddItem(result);
            this.Text = result.File.Name;
        }
    }
}