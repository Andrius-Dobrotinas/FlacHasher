﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public partial class FormX : Form
    {
        private readonly IMultipleFileHasher hasher;
        private readonly ResultsWrapper<FileHashResult, ListItem<FileHashResult>> results;
        private readonly InteractiveTextFileWriter hashFileWriter;
        private readonly FileSizeProgressBar progressReporter;
        private readonly string sourceFileFilter = "*.flac";

        public FormX(
            IMultipleFileHasher hashCalc,
            InteractiveTextFileWriter hashWriter,
            IFaceValueFactory<FileHashResult> resultListFaceValueFactory,
            FlacHash.IO.IFileReadEventSource fileReadEventSource)
        {
            InitializeComponent();

            this.results = new ResultsWrapper<FileHashResult, ListItem<FileHashResult>>(this.list_results, resultListFaceValueFactory);

            this.hasher = hashCalc;
            this.hashFileWriter = hashWriter;

            this.list_files.DisplayMember = nameof(FileInfo.Name);

            dirBrowser.ShowNewFolderButton = false;

            BuildResultsCtxMenu();

            progressReporter = new FileSizeProgressBar(progressBar);

            fileReadEventSource.BytesRead += (bytesRead) => {
                this.Invoke(new Action(() => progressReporter.Increment(bytesRead)));
            };
        }

        private void BuildResultsCtxMenu()
        {
            ctxMenu_results.Items.Add(
                "Save to a File...",
                null,
                new EventHandler((sender, e) => SaveHashes()));
        }

        private void BtnChooseDir_Click(object sender, EventArgs e)
        {
            var result = dirBrowser.ShowDialog();
            if (result != DialogResult.OK) return;

            var path = new DirectoryInfo(dirBrowser.SelectedPath);

            var files = IOUtil
                .FindFiles(path, sourceFileFilter)
                .ToArray();

            list_files.Items.Clear();
            list_files.Items.AddRange(files);
        }

        private void list_results_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            ctxMenu_results.Show(list_results, new Point(e.X, e.Y));
        }

        private void SaveHashes()
        {
            var hashes = results.GetFaceValues();

            if (hashFileWriter.GetFileAndSave(hashes) == true)
                MessageBox.Show("Hashes saved!");
        }

        private void Btn_Go_Click(object sender, EventArgs e)
        {
            var files = list_files.Items.Cast<FileInfo>();

            results.Clear();

            long totalSize = files.Select(file => file.Length).Sum();
            progressReporter.SetMaxValue(totalSize);

            Task.Factory.StartNew(() =>
            {
                CalcHashesAndUpdateUI(files);
            });
        }

        private void CalcHashesAndUpdateUI(IEnumerable<FileInfo> files)
        {
            IEnumerable<FileHashResult> results = hasher.ComputeHashes(files);

            foreach (var result in results)
            {
                //update the UI (on the UI thread)
                this.Invoke(
                    new Action(
                        () => this.results.AddResult(result)));

                this.Invoke(
                    new Action(
                        () => this.Text = result.File.Name));
            }
        }
    }
}