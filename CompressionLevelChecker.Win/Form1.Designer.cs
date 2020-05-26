namespace Andy.FlacHash.Win
{
    partial class MainForm
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
            this.BtnSelectFile = new System.Windows.Forms.Button();
            this.Lbl_Result = new System.Windows.Forms.Label();
            this.Btn_Go = new System.Windows.Forms.Button();
            this.Lbl_File = new System.Windows.Forms.Label();
            this.Group_CompressionLevel = new System.Windows.Forms.GroupBox();
            this.Lbl_CompressionLevel = new System.Windows.Forms.Label();
            this.Trackbar_CompressionLevel = new System.Windows.Forms.TrackBar();
            this.Group_DecodingMode = new System.Windows.Forms.GroupBox();
            this.Opt_Metadata_Discard = new System.Windows.Forms.RadioButton();
            this.Opt_Metadata_Keep = new System.Windows.Forms.RadioButton();
            this.Group_CompressionLevel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Trackbar_CompressionLevel)).BeginInit();
            this.Group_DecodingMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnSelectFile
            // 
            this.BtnSelectFile.Location = new System.Drawing.Point(12, 12);
            this.BtnSelectFile.Name = "BtnSelectFile";
            this.BtnSelectFile.Size = new System.Drawing.Size(245, 23);
            this.BtnSelectFile.TabIndex = 1;
            this.BtnSelectFile.Text = "Select A File";
            this.BtnSelectFile.UseVisualStyleBackColor = true;
            this.BtnSelectFile.Click += new System.EventHandler(this.ClearResult);
            // 
            // Lbl_Result
            // 
            this.Lbl_Result.AutoSize = true;
            this.Lbl_Result.Location = new System.Drawing.Point(12, 222);
            this.Lbl_Result.Name = "Lbl_Result";
            this.Lbl_Result.Size = new System.Drawing.Size(35, 13);
            this.Lbl_Result.TabIndex = 2;
            this.Lbl_Result.Text = "label1";
            // 
            // Btn_Go
            // 
            this.Btn_Go.Location = new System.Drawing.Point(15, 185);
            this.Btn_Go.Name = "Btn_Go";
            this.Btn_Go.Size = new System.Drawing.Size(242, 24);
            this.Btn_Go.TabIndex = 6;
            this.Btn_Go.Text = "Check";
            this.Btn_Go.UseVisualStyleBackColor = true;
            this.Btn_Go.Click += new System.EventHandler(this.Btn_Go_Click);
            // 
            // Lbl_File
            // 
            this.Lbl_File.AutoSize = true;
            this.Lbl_File.Location = new System.Drawing.Point(12, 48);
            this.Lbl_File.Name = "Lbl_File";
            this.Lbl_File.Size = new System.Drawing.Size(49, 13);
            this.Lbl_File.TabIndex = 7;
            this.Lbl_File.Text = "file name";
            // 
            // Group_CompressionLevel
            // 
            this.Group_CompressionLevel.Controls.Add(this.Lbl_CompressionLevel);
            this.Group_CompressionLevel.Controls.Add(this.Trackbar_CompressionLevel);
            this.Group_CompressionLevel.Location = new System.Drawing.Point(16, 74);
            this.Group_CompressionLevel.Name = "Group_CompressionLevel";
            this.Group_CompressionLevel.Size = new System.Drawing.Size(242, 64);
            this.Group_CompressionLevel.TabIndex = 8;
            this.Group_CompressionLevel.TabStop = false;
            this.Group_CompressionLevel.Text = "Target Compression level";
            // 
            // Lbl_CompressionLevel
            // 
            this.Lbl_CompressionLevel.AutoSize = true;
            this.Lbl_CompressionLevel.Location = new System.Drawing.Point(223, 16);
            this.Lbl_CompressionLevel.Name = "Lbl_CompressionLevel";
            this.Lbl_CompressionLevel.Size = new System.Drawing.Size(13, 13);
            this.Lbl_CompressionLevel.TabIndex = 8;
            this.Lbl_CompressionLevel.Text = "8";
            // 
            // Trackbar_CompressionLevel
            // 
            this.Trackbar_CompressionLevel.LargeChange = 1;
            this.Trackbar_CompressionLevel.Location = new System.Drawing.Point(6, 13);
            this.Trackbar_CompressionLevel.Name = "Trackbar_CompressionLevel";
            this.Trackbar_CompressionLevel.Size = new System.Drawing.Size(211, 45);
            this.Trackbar_CompressionLevel.TabIndex = 6;
            this.Trackbar_CompressionLevel.ValueChanged += new System.EventHandler(this.ClearResult);
            // 
            // Group_DecodingMode
            // 
            this.Group_DecodingMode.Controls.Add(this.Opt_Metadata_Discard);
            this.Group_DecodingMode.Controls.Add(this.Opt_Metadata_Keep);
            this.Group_DecodingMode.Location = new System.Drawing.Point(16, 139);
            this.Group_DecodingMode.Name = "Group_DecodingMode";
            this.Group_DecodingMode.Size = new System.Drawing.Size(241, 47);
            this.Group_DecodingMode.TabIndex = 9;
            this.Group_DecodingMode.TabStop = false;
            this.Group_DecodingMode.Text = "Recoding Mode";
            // 
            // Opt_Metadata_Discard
            // 
            this.Opt_Metadata_Discard.AutoSize = true;
            this.Opt_Metadata_Discard.Location = new System.Drawing.Point(83, 19);
            this.Opt_Metadata_Discard.Name = "Opt_Metadata_Discard";
            this.Opt_Metadata_Discard.Size = new System.Drawing.Size(112, 17);
            this.Opt_Metadata_Discard.TabIndex = 1;
            this.Opt_Metadata_Discard.TabStop = true;
            this.Opt_Metadata_Discard.Text = "Audio Stream Only";
            this.Opt_Metadata_Discard.UseVisualStyleBackColor = true;
            // 
            // Opt_Metadata_Keep
            // 
            this.Opt_Metadata_Keep.AutoSize = true;
            this.Opt_Metadata_Keep.Location = new System.Drawing.Point(6, 19);
            this.Opt_Metadata_Keep.Name = "Opt_Metadata_Keep";
            this.Opt_Metadata_Keep.Size = new System.Drawing.Size(53, 17);
            this.Opt_Metadata_Keep.TabIndex = 0;
            this.Opt_Metadata_Keep.TabStop = true;
            this.Opt_Metadata_Keep.Text = "Direct";
            this.Opt_Metadata_Keep.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 252);
            this.Controls.Add(this.Group_DecodingMode);
            this.Controls.Add(this.Group_CompressionLevel);
            this.Controls.Add(this.Lbl_File);
            this.Controls.Add(this.Btn_Go);
            this.Controls.Add(this.Lbl_Result);
            this.Controls.Add(this.BtnSelectFile);
            this.Name = "MainForm";
            this.Text = "Flac Compression Level Checker";
            this.Group_CompressionLevel.ResumeLayout(false);
            this.Group_CompressionLevel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Trackbar_CompressionLevel)).EndInit();
            this.Group_DecodingMode.ResumeLayout(false);
            this.Group_DecodingMode.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button BtnSelectFile;
        private System.Windows.Forms.Label Lbl_Result;
        private System.Windows.Forms.Button Btn_Go;
        private System.Windows.Forms.Label Lbl_File;
        private System.Windows.Forms.GroupBox Group_CompressionLevel;
        private System.Windows.Forms.Label Lbl_CompressionLevel;
        private System.Windows.Forms.TrackBar Trackbar_CompressionLevel;
        private System.Windows.Forms.GroupBox Group_DecodingMode;
        private System.Windows.Forms.RadioButton Opt_Metadata_Discard;
        private System.Windows.Forms.RadioButton Opt_Metadata_Keep;
    }
}

