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

    public class MetadataOptionsGroup
    {
        private readonly RadioButton preserve;
        private readonly RadioButton discard;

        public MetadataOptionsGroup(RadioButton preserve, RadioButton discard)
        {
            this.preserve = preserve;
            this.discard = discard;
        }

        public MetadataMode GetSelectedMode()
        {
            if (preserve.Checked) return MetadataMode.Preserve;
            if (discard.Checked) return MetadataMode.Discard;

            throw new Exception("No option is selected");
        }
    }

    public partial class MainForm : Form
    {
        private readonly CompressionLevelService compressionService;
        private readonly UI.FileOpenDialog openFileDialog;

        private readonly MetadataOptionsGroup metadataOptionsGroup;
        private System.IO.FileInfo file;

        public MainForm(
            uint maxCompressionLevel,
            uint selectedCompressionLevel,
            CompressionLevelService compressionService,
            UI.FileOpenDialog openFileDialog)
        {
            this.compressionService = compressionService ?? throw new ArgumentNullException(nameof(compressionService));
            this.openFileDialog = openFileDialog ?? throw new ArgumentNullException(nameof(openFileDialog));

            InitializeComponent();
            ExtraInitComponents();

            Trackbar_CompressionLevel.Maximum = (int)maxCompressionLevel;
            Trackbar_CompressionLevel.Value = (int)selectedCompressionLevel;

            metadataOptionsGroup = new MetadataOptionsGroup(
                this.Opt_Metadata_Keep,
                this.Opt_Metadata_Discard);
        }

        private void ExtraInitComponents()
        {
            this.BtnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);
            this.Trackbar_CompressionLevel.ValueChanged += new System.EventHandler(this.Trackbar_CompressionLevel_ValueChanged);
            this.Opt_Metadata_Keep.Checked = true;
        }

        private void Trackbar_CompressionLevel_ValueChanged(object sender, EventArgs e)
        {
            Lbl_CompressionLevel.Text = Trackbar_CompressionLevel.Value.ToString();
        }

        private void Btn_Go_Click(object sender, EventArgs e)
        {
            Lbl_Result.Text = "Checking...";

            var metadataMode = metadataOptionsGroup.GetSelectedMode();

            Range<uint> compressionLevels = compressionService.InferCompressionLevel(file, (uint)Trackbar_CompressionLevel.Value, metadataMode);

            ProcessResult(compressionLevels);
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

        private void ProcessResult(Range<uint> compressionLevels)
        {
            if (compressionLevels.IsSingleValue)
                Lbl_Result.Text = $"File compression level: {compressionLevels.MaxValue}";
            else
                Lbl_Result.Text = $"File compression level seems to be between: {compressionLevels.MinValue} and {compressionLevels.MaxValue}. The file might have metadata that the encoder omits";
        }
    }
}