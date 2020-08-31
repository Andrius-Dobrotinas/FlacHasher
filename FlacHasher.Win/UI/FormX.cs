using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public partial class FormX : Form
    {
        private readonly IMultipleFileHasher hasher;
        private readonly InteractiveTextFileWriter hashFileWriter;
        private readonly FileSizeProgressBarAdapter progressReporter;
        private readonly InteractiveDirectoryFileGetter directoryFileGetter;

        public FormX(
            IMultipleFileHasher hashCalc,
            InteractiveTextFileWriter hashFileWriter,
            IDisplayValueProducer<FileHashResult> resultListFaceValueFactory,
            IO.IFileReadEventSource fileReadEventSource,
            InteractiveDirectoryFileGetter directoryFileGetter)
        {
            InitializeComponent();

            this.list_results.DisplayValueProducer = resultListFaceValueFactory;

            this.hasher = hashCalc;
            this.hashFileWriter = hashFileWriter;
            this.directoryFileGetter = directoryFileGetter;

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
            var files = list_files.GetItems().ToList();

            this.list_results.ClearList();

            long totalSize = files.Select(file => file.Length).Sum();
            progressReporter.SetMaxValue(totalSize);

            Task.Factory.StartNew(() =>
            {
                CalcHashesAndReport(hasher, UpdateUIWithResult_OnUIThread, files);
            });
        }

        private static void CalcHashesAndReport(
            IMultipleFileHasher hasher,
            Action<FileHashResult> reportResult,
            IEnumerable<FileInfo> files)
        {
            IEnumerable<FileHashResult> results = hasher.ComputeHashes(files);

            foreach (var result in results)
            {
                reportResult(result);
            }
        }

        void UpdateUIWithResult_OnUIThread(FileHashResult result)
        {
            this.Invoke(
                new Action(
                    () => UpdateUIWithResult(result)));
        }

        private void UpdateUIWithResult(FileHashResult result)
        {
            this.list_results.AddItem(result);
            this.Text = result.File.Name;
        }
    }
}