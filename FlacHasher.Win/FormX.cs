using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Cmd
{
    public partial class FormX : Form
    {
        private readonly FileInfo decoderFile;
        private readonly SaveFileDialog saveFileDialog;

        public FormX(FileInfo decoderFile, SaveFileDialog saveFileDialog)
        {
            InitializeComponent();

            this.decoderFile = decoderFile;
            this.saveFileDialog = saveFileDialog;

            this.list_files.DisplayMember = nameof(FileInfo.Name);
            this.list_results.DisplayMember = "Hash";

            BuildResultsCtxMenu();
        }

        private void BtnChooseDir_Click(object sender, EventArgs e)
        {
            dirBrowser.ShowNewFolderButton = false;
            var result = dirBrowser.ShowDialog();
            var path = dirBrowser.SelectedPath;

            var files = Directory.GetFiles(path, "*.flac", SearchOption.TopDirectoryOnly)
                .Select(x => new FileInfo(x))
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

            var hashes = list_results.Items
                .Cast<FileHashResultListItem>()
                .Select(x => x.Hash);

            using (var outputStream = saveFileDialog.OpenFile())
            using (var writer = new StreamWriter(outputStream))
            {
                foreach (var hash in hashes)
                    writer.WriteLine(hash);
            }

            MessageBox.Show("Hashes saved!");
        }

        private void BuildResultsCtxMenu()
        {
            var ttsi = new ToolStripButton("Save As...");
            ttsi.Image = null;
            ttsi.Click += new EventHandler((sender, e) => SaveHashes());

            ctxMenu_results.Items.Add(
                "Save As...",
                null,
                new EventHandler((sender, e) => SaveHashes()));
        }

        private void Btn_Go_Click(object sender, EventArgs e)
        {
            var files = list_files.Items.Cast<FileInfo>();

            Task.Factory.StartNew(() =>
            {
                IEnumerable<FileHashResult> results = Program.DoIt(decoderFile, files);

                foreach (var result in results)
                {
                    //update the UI (on the UI thread)
                    this.list_results.Invoke(new Action(() => AddResult(result)));
                }
            });
        }

        private void AddResult(FileHashResult result)
        {
            this.list_results.Items.Add(
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