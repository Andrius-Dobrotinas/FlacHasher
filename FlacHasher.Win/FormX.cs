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

        public FormX(FileInfo decoderFile)
        {
            InitializeComponent();

            this.decoderFile = decoderFile;
            this.list_files.DisplayMember = nameof(FileInfo.Name);
            this.list_results.DisplayMember = "Hash";
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
            this.list_results.Items.Add(new
            {
                File = result.File,
                Hash = OutputFormatter.GetFormattedString("{hash}", result.Hash, result.File)
            });
        }
    }
}