using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    public delegate Tuple<long, long> WellIsIt(System.IO.FileInfo sourceFile, uint compressionLevel);

    public partial class MainForm : Form
    {
        private readonly WellIsIt wellIsIt;
        private readonly UI.FileOpenDialog openFileDialog;

        private System.IO.FileInfo file;

        public MainForm(
            uint maxCompressionLevel,
            uint selectedCompressionLevel,
            WellIsIt wellIsIt,
            UI.FileOpenDialog openFileDialog)
        {
            this.wellIsIt = wellIsIt ?? throw new ArgumentNullException(nameof(wellIsIt));
            this.openFileDialog = openFileDialog ?? throw new ArgumentNullException(nameof(openFileDialog));

            InitializeComponent();

            Trackbar_CompressionLevel.Maximum = (int)maxCompressionLevel;
            Trackbar_CompressionLevel.Value = (int)selectedCompressionLevel;
        }

        private void Trackbar_CompressionLevel_ValueChanged(object sender, EventArgs e)
        {
            Lbl_CompressionLevel.Text = Trackbar_CompressionLevel.Value.ToString();
        }

        private void Btn_Go_Click(object sender, EventArgs e)
        {
            var result = wellIsIt(file, (uint)Trackbar_CompressionLevel.Value);

            ProcessResult(result);
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            var selectedFile = openFileDialog.GetFile();
            if (selectedFile != null)
                SetNewFile(selectedFile);
        }

        private void ClearResult(object sender, EventArgs e)
        {
            Lbl_Result.Text = "Click the button";
        }

        private void SetNewFile(System.IO.FileInfo selectedFile)
        {
            file = selectedFile;
            Lbl_File.Text = file.Name;
        }

        private void ProcessResult(Tuple<long, long> result)
        {
            Lbl_Result.Text = $"Equal: {result.Item1 == result.Item2}. Source: {result.Item1}, Result: {result.Item2}";
        }
    }
}