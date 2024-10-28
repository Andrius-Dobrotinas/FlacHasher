namespace Andy.FlacHash.Application.Win.UI
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormX));
            btn_chooseDir = new System.Windows.Forms.Button();
            btn_go = new System.Windows.Forms.Button();
            ctxMenu_results = new System.Windows.Forms.ContextMenuStrip(components);
            progressBar = new System.Windows.Forms.ProgressBar();
            label_Status = new System.Windows.Forms.Label();
            layoutGroup_input = new System.Windows.Forms.TableLayoutPanel();
            list_hashFiles = new FileList();
            list_files = new FileList();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            group_Left = new System.Windows.Forms.GroupBox();
            grpModes = new System.Windows.Forms.GroupBox();
            mode_Calc = new System.Windows.Forms.RadioButton();
            mode_Verify = new System.Windows.Forms.RadioButton();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            tabs_Results = new System.Windows.Forms.TabControl();
            tabResults = new System.Windows.Forms.TabPage();
            list_results = new FileHashResultList();
            list_verification_results = new VerificationResultsList();
            col_results_verification_file = new System.Windows.Forms.ColumnHeader();
            col_results_verification_isMatch = new System.Windows.Forms.ColumnHeader();
            tabErrors = new System.Windows.Forms.TabPage();
            txtErrors = new System.Windows.Forms.TextBox();
            imgList_verification = new System.Windows.Forms.ImageList(components);
            layoutGroup_input.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            group_Left.SuspendLayout();
            grpModes.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tabs_Results.SuspendLayout();
            tabResults.SuspendLayout();
            tabErrors.SuspendLayout();
            SuspendLayout();
            // 
            // btn_chooseDir
            // 
            btn_chooseDir.Location = new System.Drawing.Point(17, 58);
            btn_chooseDir.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            btn_chooseDir.Name = "btn_chooseDir";
            btn_chooseDir.Size = new System.Drawing.Size(191, 48);
            btn_chooseDir.TabIndex = 0;
            btn_chooseDir.Text = "Choose a dir";
            btn_chooseDir.UseVisualStyleBackColor = true;
            btn_chooseDir.Click += BtnChooseDir_Click;
            // 
            // btn_go
            // 
            btn_go.Location = new System.Drawing.Point(17, 283);
            btn_go.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            btn_go.Name = "btn_go";
            btn_go.Size = new System.Drawing.Size(191, 60);
            btn_go.TabIndex = 3;
            btn_go.Text = "Go!";
            btn_go.UseVisualStyleBackColor = true;
            btn_go.Click += Btn_Go_Click;
            // 
            // ctxMenu_results
            // 
            ctxMenu_results.ImageScalingSize = new System.Drawing.Size(24, 24);
            ctxMenu_results.Name = "ctxMenu_results";
            ctxMenu_results.Size = new System.Drawing.Size(61, 4);
            // 
            // progressBar
            // 
            progressBar.Location = new System.Drawing.Point(17, 357);
            progressBar.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(191, 42);
            progressBar.TabIndex = 5;
            // 
            // label_Status
            // 
            label_Status.AutoSize = true;
            label_Status.Location = new System.Drawing.Point(28, 250);
            label_Status.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            label_Status.Name = "label_Status";
            label_Status.Size = new System.Drawing.Size(0, 25);
            label_Status.TabIndex = 6;
            // 
            // layoutGroup_input
            // 
            layoutGroup_input.ColumnCount = 1;
            layoutGroup_input.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            layoutGroup_input.Controls.Add(list_hashFiles, 0, 1);
            layoutGroup_input.Controls.Add(list_files, 0, 0);
            layoutGroup_input.Dock = System.Windows.Forms.DockStyle.Fill;
            layoutGroup_input.Location = new System.Drawing.Point(0, 0);
            layoutGroup_input.Margin = new System.Windows.Forms.Padding(0);
            layoutGroup_input.Name = "layoutGroup_input";
            layoutGroup_input.RowCount = 2;
            layoutGroup_input.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            layoutGroup_input.RowStyles.Add(new System.Windows.Forms.RowStyle());
            layoutGroup_input.Size = new System.Drawing.Size(954, 280);
            layoutGroup_input.TabIndex = 12;
            // 
            // list_hashFiles
            // 
            list_hashFiles.DisplayMember = "Name";
            list_hashFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            list_hashFiles.FormattingEnabled = true;
            list_hashFiles.ItemHeight = 25;
            list_hashFiles.Location = new System.Drawing.Point(4, 214);
            list_hashFiles.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            list_hashFiles.Name = "list_hashFiles";
            list_hashFiles.Size = new System.Drawing.Size(946, 61);
            list_hashFiles.TabIndex = 14;
            // 
            // list_files
            // 
            list_files.DisplayMember = "Name";
            list_files.Dock = System.Windows.Forms.DockStyle.Fill;
            list_files.FormattingEnabled = true;
            list_files.ItemHeight = 25;
            list_files.Location = new System.Drawing.Point(6, 5);
            list_files.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            list_files.Name = "list_files";
            list_files.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            list_files.Size = new System.Drawing.Size(942, 199);
            list_files.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 229F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(group_Left, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(1191, 570);
            tableLayoutPanel1.TabIndex = 13;
            // 
            // group_Left
            // 
            group_Left.Controls.Add(grpModes);
            group_Left.Controls.Add(btn_chooseDir);
            group_Left.Controls.Add(label_Status);
            group_Left.Controls.Add(btn_go);
            group_Left.Controls.Add(progressBar);
            group_Left.Dock = System.Windows.Forms.DockStyle.Fill;
            group_Left.Location = new System.Drawing.Point(4, 5);
            group_Left.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            group_Left.Name = "group_Left";
            group_Left.Padding = new System.Windows.Forms.Padding(0);
            group_Left.Size = new System.Drawing.Size(221, 560);
            group_Left.TabIndex = 0;
            group_Left.TabStop = false;
            // 
            // grpModes
            // 
            grpModes.Controls.Add(mode_Calc);
            grpModes.Controls.Add(mode_Verify);
            grpModes.Location = new System.Drawing.Point(17, 114);
            grpModes.Name = "grpModes";
            grpModes.Size = new System.Drawing.Size(191, 111);
            grpModes.TabIndex = 1;
            grpModes.TabStop = false;
            // 
            // mode_Calc
            // 
            mode_Calc.AutoSize = true;
            mode_Calc.Location = new System.Drawing.Point(14, 20);
            mode_Calc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            mode_Calc.Name = "mode_Calc";
            mode_Calc.Size = new System.Drawing.Size(77, 29);
            mode_Calc.TabIndex = 1;
            mode_Calc.TabStop = true;
            mode_Calc.Text = "Hash";
            mode_Calc.UseVisualStyleBackColor = true;
            mode_Calc.CheckedChanged += mode_Calc_CheckedChanged;
            // 
            // mode_Verify
            // 
            mode_Verify.AutoSize = true;
            mode_Verify.Location = new System.Drawing.Point(14, 61);
            mode_Verify.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            mode_Verify.Name = "mode_Verify";
            mode_Verify.Size = new System.Drawing.Size(81, 29);
            mode_Verify.TabIndex = 2;
            mode_Verify.TabStop = true;
            mode_Verify.Text = "Verify";
            mode_Verify.UseVisualStyleBackColor = true;
            mode_Verify.CheckedChanged += mode_Verify_CheckedChanged;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(tabs_Results, 0, 1);
            tableLayoutPanel2.Controls.Add(layoutGroup_input, 0, 0);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(233, 5);
            tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new System.Drawing.Size(954, 560);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // tabs_Results
            // 
            tabs_Results.Controls.Add(tabResults);
            tabs_Results.Controls.Add(tabErrors);
            tabs_Results.Dock = System.Windows.Forms.DockStyle.Fill;
            tabs_Results.Location = new System.Drawing.Point(0, 280);
            tabs_Results.Margin = new System.Windows.Forms.Padding(0);
            tabs_Results.Name = "tabs_Results";
            tabs_Results.SelectedIndex = 0;
            tabs_Results.Size = new System.Drawing.Size(954, 280);
            tabs_Results.TabIndex = 13;
            // 
            // tabResults
            // 
            tabResults.Controls.Add(list_results);
            tabResults.Controls.Add(list_verification_results);
            tabResults.Location = new System.Drawing.Point(4, 34);
            tabResults.Margin = new System.Windows.Forms.Padding(0);
            tabResults.Name = "tabResults";
            tabResults.Size = new System.Drawing.Size(946, 242);
            tabResults.TabIndex = 0;
            tabResults.Text = "Results";
            tabResults.UseVisualStyleBackColor = true;
            // 
            // list_results
            // 
            list_results.DisplayMember = "HashString";
            list_results.Dock = System.Windows.Forms.DockStyle.Fill;
            list_results.FormattingEnabled = true;
            list_results.ItemHeight = 25;
            list_results.Location = new System.Drawing.Point(0, 0);
            list_results.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            list_results.Name = "list_results";
            list_results.Size = new System.Drawing.Size(946, 242);
            list_results.TabIndex = 12;
            // 
            // list_verification_results
            // 
            list_verification_results.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { col_results_verification_file, col_results_verification_isMatch });
            list_verification_results.Dock = System.Windows.Forms.DockStyle.Fill;
            list_verification_results.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            list_verification_results.HideSelection = false;
            list_verification_results.Location = new System.Drawing.Point(0, 0);
            list_verification_results.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            list_verification_results.Name = "list_verification_results";
            list_verification_results.Size = new System.Drawing.Size(946, 242);
            list_verification_results.TabIndex = 11;
            list_verification_results.UseCompatibleStateImageBehavior = false;
            // 
            // col_results_verification_file
            // 
            col_results_verification_file.Text = "File";
            col_results_verification_file.Width = 200;
            // 
            // col_results_verification_isMatch
            // 
            col_results_verification_isMatch.Text = "Matches";
            // 
            // tabErrors
            // 
            tabErrors.Controls.Add(txtErrors);
            tabErrors.Location = new System.Drawing.Point(4, 34);
            tabErrors.Margin = new System.Windows.Forms.Padding(0);
            tabErrors.Name = "tabErrors";
            tabErrors.Size = new System.Drawing.Size(946, 242);
            tabErrors.TabIndex = 1;
            tabErrors.Text = "Errors";
            tabErrors.UseVisualStyleBackColor = true;
            // 
            // txtErrors
            // 
            txtErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            txtErrors.HideSelection = false;
            txtErrors.Location = new System.Drawing.Point(0, 0);
            txtErrors.Multiline = true;
            txtErrors.Name = "txtErrors";
            txtErrors.ReadOnly = true;
            txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtErrors.Size = new System.Drawing.Size(946, 242);
            txtErrors.TabIndex = 0;
            // 
            // imgList_verification
            // 
            imgList_verification.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imgList_verification.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imgList_verification.ImageStream");
            imgList_verification.TransparentColor = System.Drawing.Color.Transparent;
            imgList_verification.Images.SetKeyName(0, "bad");
            imgList_verification.Images.SetKeyName(1, "good");
            imgList_verification.Images.SetKeyName(2, "error.png");
            // 
            // FormX
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1191, 570);
            Controls.Add(tableLayoutPanel1);
            Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            Name = "FormX";
            Text = "FormX";
            layoutGroup_input.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            group_Left.ResumeLayout(false);
            group_Left.PerformLayout();
            grpModes.ResumeLayout(false);
            grpModes.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tabs_Results.ResumeLayout(false);
            tabResults.ResumeLayout(false);
            tabErrors.ResumeLayout(false);
            tabErrors.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btn_chooseDir;
        private System.Windows.Forms.Button btn_go;
        private System.Windows.Forms.ContextMenuStrip ctxMenu_results;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.TableLayoutPanel layoutGroup_input;
        private FileList list_hashFiles;
        private FileList list_files;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox group_Left;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ImageList imgList_verification;
        private System.Windows.Forms.GroupBox grpModes;
        private System.Windows.Forms.RadioButton mode_Calc;
        private System.Windows.Forms.RadioButton mode_Verify;
        private System.Windows.Forms.TabControl tabs_Results;
        private System.Windows.Forms.TabPage tabResults;
        private FileHashResultList list_results;
        private VerificationResultsList list_verification_results;
        private System.Windows.Forms.ColumnHeader col_results_verification_file;
        private System.Windows.Forms.ColumnHeader col_results_verification_isMatch;
        private System.Windows.Forms.TabPage tabErrors;
        private System.Windows.Forms.TextBox txtErrors;
    }
}