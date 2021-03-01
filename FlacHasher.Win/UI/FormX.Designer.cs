namespace Andy.FlacHash.Win.UI
{
    partial class FormX
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
            this.components = new System.ComponentModel.Container();
            this.btn_chooseDir = new System.Windows.Forms.Button();
            this.list_files = new Andy.FlacHash.Win.UI.FileList();
            this.btn_go = new System.Windows.Forms.Button();
            this.list_results = new Andy.FlacHash.Win.UI.FileHashResultList();
            this.ctxMenu_results = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.label_Status = new System.Windows.Forms.Label();
            this.mode_Verify = new System.Windows.Forms.RadioButton();
            this.mode_Calc = new System.Windows.Forms.RadioButton();
            this.list_verification_results = new System.Windows.Forms.ListView();
            this.col_file = new System.Windows.Forms.ColumnHeader();
            this.col_isMatch = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // btn_chooseDir
            // 
            this.btn_chooseDir.Location = new System.Drawing.Point(14, 10);
            this.btn_chooseDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btn_chooseDir.Name = "btn_chooseDir";
            this.btn_chooseDir.Size = new System.Drawing.Size(139, 29);
            this.btn_chooseDir.TabIndex = 0;
            this.btn_chooseDir.Text = "Choose a dir";
            this.btn_chooseDir.UseVisualStyleBackColor = true;
            this.btn_chooseDir.Click += new System.EventHandler(this.BtnChooseDir_Click);
            // 
            // list_files
            // 
            this.list_files.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.list_files.DisplayMember = "Name";
            this.list_files.FormattingEnabled = true;
            this.list_files.ItemHeight = 15;
            this.list_files.Location = new System.Drawing.Point(160, 10);
            this.list_files.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.list_files.Name = "list_files";
            this.list_files.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.list_files.Size = new System.Drawing.Size(660, 139);
            this.list_files.TabIndex = 2;
            // 
            // btn_go
            // 
            this.btn_go.Location = new System.Drawing.Point(18, 143);
            this.btn_go.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btn_go.Name = "btn_go";
            this.btn_go.Size = new System.Drawing.Size(134, 36);
            this.btn_go.TabIndex = 3;
            this.btn_go.Text = "Go!";
            this.btn_go.UseVisualStyleBackColor = true;
            this.btn_go.Click += new System.EventHandler(this.Btn_Go_Click);
            // 
            // list_results
            // 
            this.list_results.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.list_results.DisplayMember = "DisplayValue";
            this.list_results.DisplayValueProducer = null;
            this.list_results.FormattingEnabled = true;
            this.list_results.ItemHeight = 15;
            this.list_results.Location = new System.Drawing.Point(160, 187);
            this.list_results.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.list_results.Name = "list_results";
            this.list_results.Size = new System.Drawing.Size(660, 139);
            this.list_results.TabIndex = 4;
            // 
            // ctxMenu_results
            // 
            this.ctxMenu_results.Name = "ctxMenu_results";
            this.ctxMenu_results.Size = new System.Drawing.Size(61, 4);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(18, 187);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(134, 25);
            this.progressBar.TabIndex = 5;
            // 
            // label_Status
            // 
            this.label_Status.AutoSize = true;
            this.label_Status.Location = new System.Drawing.Point(160, 158);
            this.label_Status.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Status.Name = "label_Status";
            this.label_Status.Size = new System.Drawing.Size(0, 15);
            this.label_Status.TabIndex = 6;
            // 
            // mode_Verify
            // 
            this.mode_Verify.AutoSize = true;
            this.mode_Verify.Location = new System.Drawing.Point(18, 71);
            this.mode_Verify.Name = "mode_Verify";
            this.mode_Verify.Size = new System.Drawing.Size(54, 19);
            this.mode_Verify.TabIndex = 8;
            this.mode_Verify.TabStop = true;
            this.mode_Verify.Text = "Verify";
            this.mode_Verify.UseVisualStyleBackColor = true;
            this.mode_Verify.CheckedChanged += new System.EventHandler(this.mode_Verify_CheckedChanged);
            // 
            // mode_Calc
            // 
            this.mode_Calc.AutoSize = true;
            this.mode_Calc.Location = new System.Drawing.Point(18, 46);
            this.mode_Calc.Name = "mode_Calc";
            this.mode_Calc.Size = new System.Drawing.Size(74, 19);
            this.mode_Calc.TabIndex = 7;
            this.mode_Calc.TabStop = true;
            this.mode_Calc.Text = "Calculate";
            this.mode_Calc.UseVisualStyleBackColor = true;
            this.mode_Calc.CheckedChanged += new System.EventHandler(this.mode_Calc_CheckedChanged);
            // 
            // list_verification_results
            // 
            this.list_verification_results.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.col_file,
            this.col_isMatch});
            this.list_verification_results.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.list_verification_results.HideSelection = false;
            this.list_verification_results.Location = new System.Drawing.Point(160, 187);
            this.list_verification_results.Name = "list_verification_results";
            this.list_verification_results.Size = new System.Drawing.Size(660, 139);
            this.list_verification_results.TabIndex = 10;
            this.list_verification_results.UseCompatibleStateImageBehavior = false;
            // 
            // col_file
            // 
            this.col_file.Text = "File";
            // 
            // col_isMatch
            // 
            this.col_isMatch.Text = "Matches";
            // 
            // FormX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 342);
            this.Controls.Add(this.list_verification_results);
            this.Controls.Add(this.mode_Verify);
            this.Controls.Add(this.mode_Calc);
            this.Controls.Add(this.label_Status);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.list_results);
            this.Controls.Add(this.btn_go);
            this.Controls.Add(this.list_files);
            this.Controls.Add(this.btn_chooseDir);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormX";
            this.Text = "FormX";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_chooseDir;
        private FileList list_files;
        private System.Windows.Forms.Button btn_go;
        private FileHashResultList list_results;
        private System.Windows.Forms.ContextMenuStrip ctxMenu_results;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.RadioButton mode_Verify;
        private System.Windows.Forms.RadioButton mode_Calc;
        private System.Windows.Forms.ListView list_verification_results;
        private System.Windows.Forms.ColumnHeader col_file;
        private System.Windows.Forms.ColumnHeader col_isMatch;
    }
}