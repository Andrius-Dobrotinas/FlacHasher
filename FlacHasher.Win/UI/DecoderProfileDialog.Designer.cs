namespace Andy.FlacHash.Application.Win.UI
{
    partial class DecoderProfileDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblName = new Label();
            txtName = new TextBox();
            lblDecoder = new Label();
            txtDecoder = new TextBox();
            btnBrowse = new Button();
            lblDecoderParameters = new Label();
            txtDecoderParameters = new TextBox();
            lblTargetFileExtensions = new Label();
            txtTargetFileExtensions = new TextBox();
            btnOk = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(12, 15);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(42, 15);
            lblName.TabIndex = 0;
            lblName.Text = "Name:";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new System.Drawing.Point(12, 33);
            txtName.Name = "txtName";
            txtName.Size = new System.Drawing.Size(461, 23);
            txtName.TabIndex = 1;
            txtName.TextChanged += TxtName_TextChanged;
            // 
            // lblDecoder
            // 
            lblDecoder.AutoSize = true;
            lblDecoder.Location = new System.Drawing.Point(12, 65);
            lblDecoder.Name = "lblDecoder";
            lblDecoder.Size = new System.Drawing.Size(56, 15);
            lblDecoder.TabIndex = 2;
            lblDecoder.Text = "Decoder:";
            // 
            // txtDecoder
            // 
            txtDecoder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDecoder.Location = new System.Drawing.Point(12, 83);
            txtDecoder.Name = "txtDecoder";
            txtDecoder.Size = new System.Drawing.Size(380, 23);
            txtDecoder.TabIndex = 3;
            txtDecoder.TextChanged += TxtDecoder_TextChanged;
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Location = new System.Drawing.Point(398, 82);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new System.Drawing.Size(75, 25);
            btnBrowse.TabIndex = 4;
            btnBrowse.Text = "Browse...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // lblDecoderParameters
            // 
            lblDecoderParameters.AutoSize = true;
            lblDecoderParameters.Location = new System.Drawing.Point(12, 115);
            lblDecoderParameters.Name = "lblDecoderParameters";
            lblDecoderParameters.Size = new System.Drawing.Size(124, 15);
            lblDecoderParameters.TabIndex = 5;
            lblDecoderParameters.Text = "Decoder Parameters:";
            // 
            // txtDecoderParameters
            // 
            txtDecoderParameters.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDecoderParameters.Location = new System.Drawing.Point(12, 133);
            txtDecoderParameters.Name = "txtDecoderParameters";
            txtDecoderParameters.Size = new System.Drawing.Size(461, 23);
            txtDecoderParameters.TabIndex = 6;
            txtDecoderParameters.TextChanged += TxtDecoderParameters_TextChanged;
            // 
            // lblTargetFileExtensions
            // 
            lblTargetFileExtensions.AutoSize = true;
            lblTargetFileExtensions.Location = new System.Drawing.Point(12, 165);
            lblTargetFileExtensions.Name = "lblTargetFileExtensions";
            lblTargetFileExtensions.Size = new System.Drawing.Size(133, 15);
            lblTargetFileExtensions.TabIndex = 7;
            lblTargetFileExtensions.Text = "Target File Extensions:";
            // 
            // txtTargetFileExtensions
            // 
            txtTargetFileExtensions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTargetFileExtensions.Location = new System.Drawing.Point(12, 183);
            txtTargetFileExtensions.Name = "txtTargetFileExtensions";
            txtTargetFileExtensions.Size = new System.Drawing.Size(461, 23);
            txtTargetFileExtensions.TabIndex = 8;
            txtTargetFileExtensions.TextChanged += TxtTargetFileExtensions_TextChanged;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.Enabled = false;
            btnOk.Location = new System.Drawing.Point(317, 225);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(75, 25);
            btnOk.TabIndex = 9;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += BtnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(398, 225);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 25);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // DecoderProfileDialog
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(485, 262);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblDecoder);
            Controls.Add(txtDecoder);
            Controls.Add(btnBrowse);
            Controls.Add(lblDecoderParameters);
            Controls.Add(txtDecoderParameters);
            Controls.Add(lblTargetFileExtensions);
            Controls.Add(txtTargetFileExtensions);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DecoderProfileDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Decoder Profile Configuration";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblName;
        private TextBox txtName;
        private Label lblDecoder;
        private TextBox txtDecoder;
        private Button btnBrowse;
        private Label lblDecoderParameters;
        private TextBox txtDecoderParameters;
        private Label lblTargetFileExtensions;
        private TextBox txtTargetFileExtensions;
        private Button btnOk;
        private Button btnCancel;
    }
}

