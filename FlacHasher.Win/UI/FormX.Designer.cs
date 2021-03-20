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
            this.btn_go = new System.Windows.Forms.Button();
            this.list_results = new Andy.FlacHash.Win.UI.FileHashResultList();
            this.ctxMenu_results = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.label_Status = new System.Windows.Forms.Label();
            this.mode_Verify = new System.Windows.Forms.RadioButton();
            this.mode_Calc = new System.Windows.Forms.RadioButton();
            this.list_verification_results = new Andy.FlacHash.Win.UI.VerificationResultsList();
            this.col_results_verification_file = new System.Windows.Forms.ColumnHeader();
            this.col_results_verification_isMatch = new System.Windows.Forms.ColumnHeader();
            this.layoutGroup_input = new System.Windows.Forms.TableLayoutPanel();
            this.list_hashFiles = new Andy.FlacHash.Win.UI.FileList();
            this.list_files = new Andy.FlacHash.Win.UI.FileList();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.group_Left = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.group_Results = new System.Windows.Forms.GroupBox();
            this.layoutGroup_input.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.group_Left.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.group_Results.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_chooseDir
            // 
            this.btn_chooseDir.Location = new System.Drawing.Point(12, 35);
            this.btn_chooseDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btn_chooseDir.Name = "btn_chooseDir";
            this.btn_chooseDir.Size = new System.Drawing.Size(134, 29);
            this.btn_chooseDir.TabIndex = 0;
            this.btn_chooseDir.Text = "Choose a dir";
            this.btn_chooseDir.UseVisualStyleBackColor = true;
            this.btn_chooseDir.Click += new System.EventHandler(this.BtnChooseDir_Click);
            // 
            // btn_go
            // 
            this.btn_go.Location = new System.Drawing.Point(12, 170);
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
            this.list_results.DisplayMember = "HashString";
            this.list_results.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list_results.FormattingEnabled = true;
            this.list_results.ItemHeight = 15;
            this.list_results.Location = new System.Drawing.Point(0, 16);
            this.list_results.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.list_results.Name = "list_results";
            this.list_results.Size = new System.Drawing.Size(662, 146);
            this.list_results.TabIndex = 4;
            // 
            // ctxMenu_results
            // 
            this.ctxMenu_results.Name = "ctxMenu_results";
            this.ctxMenu_results.Size = new System.Drawing.Size(61, 4);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 214);
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
            this.mode_Verify.Location = new System.Drawing.Point(12, 95);
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
            this.mode_Calc.Location = new System.Drawing.Point(12, 70);
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
            this.col_results_verification_file,
            this.col_results_verification_isMatch});
            this.list_verification_results.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list_verification_results.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.list_verification_results.HideSelection = false;
            this.list_verification_results.Location = new System.Drawing.Point(0, 16);
            this.list_verification_results.Name = "list_verification_results";
            this.list_verification_results.Size = new System.Drawing.Size(662, 146);
            this.list_verification_results.TabIndex = 10;
            this.list_verification_results.UseCompatibleStateImageBehavior = false;
            // 
            // col_results_verification_file
            // 
            this.col_results_verification_file.Text = "File";
            // 
            // col_results_verification_isMatch
            // 
            this.col_results_verification_isMatch.Text = "Matches";
            // 
            // layoutGroup_input
            // 
            this.layoutGroup_input.ColumnCount = 1;
            this.layoutGroup_input.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutGroup_input.Controls.Add(this.list_hashFiles, 0, 1);
            this.layoutGroup_input.Controls.Add(this.list_files, 0, 0);
            this.layoutGroup_input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutGroup_input.Location = new System.Drawing.Point(0, 0);
            this.layoutGroup_input.Margin = new System.Windows.Forms.Padding(0);
            this.layoutGroup_input.Name = "layoutGroup_input";
            this.layoutGroup_input.RowCount = 2;
            this.layoutGroup_input.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutGroup_input.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutGroup_input.Size = new System.Drawing.Size(668, 168);
            this.layoutGroup_input.TabIndex = 12;
            // 
            // list_hashFiles
            // 
            this.list_hashFiles.DisplayMember = "Name";
            this.list_hashFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list_hashFiles.FormattingEnabled = true;
            this.list_hashFiles.ItemHeight = 15;
            this.list_hashFiles.Location = new System.Drawing.Point(3, 127);
            this.list_hashFiles.Name = "list_hashFiles";
            this.list_hashFiles.Size = new System.Drawing.Size(662, 38);
            this.list_hashFiles.TabIndex = 14;
            // 
            // list_files
            // 
            this.list_files.DisplayMember = "Name";
            this.list_files.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list_files.FormattingEnabled = true;
            this.list_files.ItemHeight = 15;
            this.list_files.Location = new System.Drawing.Point(4, 3);
            this.list_files.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.list_files.Name = "list_files";
            this.list_files.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.list_files.Size = new System.Drawing.Size(660, 118);
            this.list_files.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.group_Left, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(834, 342);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // group_Left
            // 
            this.group_Left.Controls.Add(this.btn_chooseDir);
            this.group_Left.Controls.Add(this.mode_Calc);
            this.group_Left.Controls.Add(this.btn_go);
            this.group_Left.Controls.Add(this.progressBar);
            this.group_Left.Controls.Add(this.mode_Verify);
            this.group_Left.Dock = System.Windows.Forms.DockStyle.Fill;
            this.group_Left.Location = new System.Drawing.Point(3, 3);
            this.group_Left.Name = "group_Left";
            this.group_Left.Padding = new System.Windows.Forms.Padding(0);
            this.group_Left.Size = new System.Drawing.Size(154, 336);
            this.group_Left.TabIndex = 0;
            this.group_Left.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.layoutGroup_input, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.group_Results, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(163, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(668, 336);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // group_Results
            // 
            this.group_Results.Controls.Add(this.list_verification_results);
            this.group_Results.Controls.Add(this.list_results);
            this.group_Results.Dock = System.Windows.Forms.DockStyle.Fill;
            this.group_Results.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.group_Results.Location = new System.Drawing.Point(3, 171);
            this.group_Results.Name = "group_Results";
            this.group_Results.Padding = new System.Windows.Forms.Padding(0);
            this.group_Results.Size = new System.Drawing.Size(662, 162);
            this.group_Results.TabIndex = 13;
            this.group_Results.TabStop = false;
            // 
            // FormX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 342);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label_Status);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormX";
            this.Text = "FormX";
            this.layoutGroup_input.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.group_Left.ResumeLayout(false);
            this.group_Left.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.group_Results.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_chooseDir;
        private System.Windows.Forms.Button btn_go;
        private FileHashResultList list_results;
        private System.Windows.Forms.ContextMenuStrip ctxMenu_results;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.RadioButton mode_Verify;
        private System.Windows.Forms.RadioButton mode_Calc;
        private VerificationResultsList list_verification_results;
        private System.Windows.Forms.ColumnHeader col_results_verification_file;
        private System.Windows.Forms.ColumnHeader col_results_verification_isMatch;
        private System.Windows.Forms.TableLayoutPanel layoutGroup_input;
        private FileList list_hashFiles;
        private FileList list_files;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox group_Left;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox group_Results;
    }
}