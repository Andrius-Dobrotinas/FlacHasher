using Andy.FlacHash.Cmd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    public partial class FormX : Form
    {
        private readonly FileInfo decoderFile;
        private readonly SaveFileDialog saveFileDialog;
        private readonly string sourceFileFilter = "*.flac";

        public FormX(FileInfo decoderFile, SaveFileDialog saveFileDialog)
        {
            InitializeComponent();

            this.decoderFile = decoderFile;
            this.saveFileDialog = saveFileDialog;

            this.list_files.DisplayMember = nameof(FileInfo.Name);
            this.list_results.DisplayMember = nameof(FileHashResultListItem.Hash);

            dirBrowser.ShowNewFolderButton = false;

            BuildResultsCtxMenu();
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

            list_files.Items.AddRange(files);   
        }

        private void list_results_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            ctxMenu_results.Show(list_results, new Point(e.X, e.Y));
        }

        private void SaveHashes()
        {
            var result = saveFileDialog.ShowDialog();
            if (result != DialogResult.OK) return;

            var hashes = GetHashes();

            IOUtil.WriteToFile(new FileInfo(saveFileDialog.FileName), hashes);

            MessageBox.Show("Hashes saved!");
        }

        private IEnumerable<string> GetHashes()
        {
            return list_results.Items
                .Cast<FileHashResultListItem>()
                .Select(x => x.Hash);
        }

        private void Btn_Go_Click(object sender, EventArgs e)
        {
            var files = list_files.Items.Cast<FileInfo>();

            Task.Factory.StartNew(() =>
            {
                CalcHashesAndUpdateUI(files);
            });
        }

        private void CalcHashesAndUpdateUI(IEnumerable<FileInfo> files)
        {
            IEnumerable<FileHashResult> results = Program.DoIt(decoderFile, files);

            foreach (var result in results)
            {
                //update the UI (on the UI thread)
                this.Invoke(new Action(() => AddResult(result)));
            }
        }

        private void AddResult(FileHashResult result)
        {
            list_results.Items.Add(
                new FileHashResultListItem
                {
                    File = result.File,
                    Hash = OutputFormatter.GetFormattedString("{hash}", result.Hash, result.File)
                });
        }

        public class FileHashResultListItem
        {
            public FileInfo File { get; set; }
            public string Hash { get; set; }
        }
    }
}