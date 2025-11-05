using System;
using System.Linq;
using System.Windows.Forms;
using Andy.Cmd.Parameter;
using Andy.FlacHash.Application.Audio;
using Andy.FlacHash.Application.Cmd;

namespace Andy.FlacHash.Application.Win.UI
{
    public partial class DecoderProfileDialog : Form
    {
        public DecoderProfile Profile { get; private set; }

        private readonly DecoderProfile _initialProfile;

        public DecoderProfileDialog()
            : this(null)
        {
        }

        public DecoderProfileDialog(DecoderProfile profile)
        {
            InitializeComponent();
            _initialProfile = profile;
            SetHelpTexts();
            if (_initialProfile != null)
            {
                PopulateFieldsFromProfile(_initialProfile);
            }
            UpdateOkButtonState();
        }

        // New: populate fields from profile
        private void PopulateFieldsFromProfile(DecoderProfile profile)
        {
            txtName.Text = profile.Name;
            txtDecoder.Text = profile.Decoder;
            txtDecoderParameters.Text = profile.DecoderParameters != null ? string.Join(";", profile.DecoderParameters) : null;
            txtTargetFileExtensions.Text = profile.TargetFileExtensions != null ? string.Join(";", profile.TargetFileExtensions) : null;
        }

        private void SetHelpTexts()
        {
            var decoderDescription = ParameterDescriptionAttribute.GetDescription<MasterParameters>(x => x.DecoderExe);
            lblDecoderHelp.Text = decoderDescription ?? string.Empty;

            var decoderParametersDescription = ParameterDescriptionAttribute.GetDescription<MasterParameters>(x => x.DecoderParameters);
            lblDecoderParametersHelp.Text = decoderParametersDescription ?? string.Empty;

            var targetFileExtensionsDescription = ParameterDescriptionAttribute.GetDescription<MasterParameters>(x => x.TargetFileExtensions);
            lblTargetFileExtensionsHelp.Text = targetFileExtensionsDescription ?? string.Empty;
        }

        private void UpdateOkButtonState()
        {
            btnOk.Enabled = !string.IsNullOrWhiteSpace(txtName.Text) &&
                           !string.IsNullOrWhiteSpace(txtDecoder.Text) &&
                           !string.IsNullOrWhiteSpace(txtDecoderParameters.Text) &&
                           !string.IsNullOrWhiteSpace(txtTargetFileExtensions.Text);
        }

        private void TxtName_TextChanged(object sender, EventArgs e)
        {
            UpdateOkButtonState();
        }

        private void TxtDecoder_TextChanged(object sender, EventArgs e)
        {
            UpdateOkButtonState();
        }

        private void TxtDecoderParameters_TextChanged(object sender, EventArgs e)
        {
            UpdateOkButtonState();
        }

        private void TxtTargetFileExtensions_TextChanged(object sender, EventArgs e)
        {
            UpdateOkButtonState();
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable files|*.exe|All files|*.*";
                openFileDialog.Title = "Select Decoder Executable";
                openFileDialog.CheckPathExists = true;
                openFileDialog.CheckFileExists = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtDecoder.Text = openFileDialog.FileName;
                }
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var decoderText = txtDecoder.Text.Trim();
            var resolved = AudioDecoder.ResolveDecoder(decoderText);
            if (resolved == null)
            {
                MessageBox.Show(this, $"Decoder not found: {decoderText}", "Invalid decoder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDecoder.Focus();
                txtDecoder.SelectAll();
                return;
            }

            var decoderParameters = txtDecoderParameters.Text
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();

            var targetFileExtensions = txtTargetFileExtensions.Text
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim())
                .Where(ext => !string.IsNullOrEmpty(ext))
                .ToArray();

            // Ensure all fields are populated
            Profile = new DecoderProfile
            {
                Name = txtName.Text.Trim(),
                Decoder = txtDecoder.Text.Trim(),
                DecoderParameters = decoderParameters ?? new string[0],
                TargetFileExtensions = targetFileExtensions ?? new string[0]
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

