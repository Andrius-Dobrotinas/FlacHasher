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
            btn_go = new System.Windows.Forms.Button();
            ctxMenu_results = new System.Windows.Forms.ContextMenuStrip(components);
            progressBar = new System.Windows.Forms.ProgressBar();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            group_Left = new System.Windows.Forms.GroupBox();
            groupVerification = new System.Windows.Forms.GroupBox();
            btn_openHashfile = new System.Windows.Forms.Button();
            groupSettings = new System.Windows.Forms.GroupBox();
            menu_hashingAlgorithm = new System.Windows.Forms.ComboBox();
            menu_decoderProfiles = new System.Windows.Forms.ComboBox();
            groupHashing = new System.Windows.Forms.GroupBox();
            grp_hashing_dir = new System.Windows.Forms.GroupBox();
            menu_fileExtensions = new System.Windows.Forms.ComboBox();
            btn_chooseDir = new System.Windows.Forms.Button();
            btn_chooseFiles = new System.Windows.Forms.Button();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            txtStatus = new System.Windows.Forms.TextBox();
            groupFiles = new System.Windows.Forms.GroupBox();
            list_verification_results = new VerificationResultsList();
            col_results_verification_file = new System.Windows.Forms.ColumnHeader();
            col_results_verification_isMatch = new System.Windows.Forms.ColumnHeader();
            imgList_verification = new System.Windows.Forms.ImageList(components);
            list_results = new FileHashResultList();
            columnHashResult_File = new System.Windows.Forms.ColumnHeader();
            columnHashResult_Hash = new System.Windows.Forms.ColumnHeader();
            tableLayoutPanel1.SuspendLayout();
            group_Left.SuspendLayout();
            groupVerification.SuspendLayout();
            groupSettings.SuspendLayout();
            groupHashing.SuspendLayout();
            grp_hashing_dir.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            groupFiles.SuspendLayout();
            SuspendLayout();
            // 
            // btn_go
            // 
            btn_go.Location = new System.Drawing.Point(15, 426);
            btn_go.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            btn_go.Name = "btn_go";
            btn_go.Size = new System.Drawing.Size(191, 60);
            btn_go.TabIndex = 9;
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
            progressBar.Location = new System.Drawing.Point(15, 500);
            progressBar.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(191, 42);
            progressBar.TabIndex = 5;
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
            group_Left.Controls.Add(groupVerification);
            group_Left.Controls.Add(groupSettings);
            group_Left.Controls.Add(groupHashing);
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
            // groupVerification
            // 
            groupVerification.Controls.Add(btn_openHashfile);
            groupVerification.Location = new System.Drawing.Point(0, 220);
            groupVerification.Name = "groupVerification";
            groupVerification.Size = new System.Drawing.Size(221, 90);
            groupVerification.TabIndex = 4;
            groupVerification.TabStop = false;
            groupVerification.Text = "Verification";
            // 
            // btn_openHashfile
            // 
            btn_openHashfile.Location = new System.Drawing.Point(15, 32);
            btn_openHashfile.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            btn_openHashfile.Name = "btn_openHashfile";
            btn_openHashfile.Size = new System.Drawing.Size(191, 48);
            btn_openHashfile.TabIndex = 5;
            btn_openHashfile.Text = "Choose a Hashfile";
            btn_openHashfile.UseVisualStyleBackColor = true;
            btn_openHashfile.Click += BtnChooseHashfile_Click;
            // 
            // groupSettings
            // 
            groupSettings.Controls.Add(menu_hashingAlgorithm);
            groupSettings.Controls.Add(menu_decoderProfiles);
            groupSettings.Location = new System.Drawing.Point(0, 297);
            groupSettings.Name = "groupSettings";
            groupSettings.Size = new System.Drawing.Size(221, 110);
            groupSettings.TabIndex = 4;
            groupSettings.TabStop = false;
            // 
            // menu_hashingAlgorithm
            // 
            menu_hashingAlgorithm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            menu_hashingAlgorithm.FormattingEnabled = true;
            menu_hashingAlgorithm.Location = new System.Drawing.Point(15, 65);
            menu_hashingAlgorithm.Name = "menu_hashingAlgorithm";
            menu_hashingAlgorithm.Size = new System.Drawing.Size(191, 33);
            menu_hashingAlgorithm.TabIndex = 8;
            // 
            // menu_decoderProfiles
            // 
            menu_decoderProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            menu_decoderProfiles.FormattingEnabled = true;
            menu_decoderProfiles.Location = new System.Drawing.Point(15, 23);
            menu_decoderProfiles.Name = "menu_decoderProfiles";
            menu_decoderProfiles.Size = new System.Drawing.Size(191, 33);
            menu_decoderProfiles.TabIndex = 7;
            // 
            // groupHashing
            // 
            groupHashing.Controls.Add(grp_hashing_dir);
            groupHashing.Controls.Add(btn_chooseFiles);
            groupHashing.Location = new System.Drawing.Point(0, 0);
            groupHashing.Name = "groupHashing";
            groupHashing.Size = new System.Drawing.Size(221, 221);
            groupHashing.TabIndex = 0;
            groupHashing.TabStop = false;
            groupHashing.Text = "Hashing";
            // 
            // grp_hashing_dir
            // 
            grp_hashing_dir.Controls.Add(menu_fileExtensions);
            grp_hashing_dir.Controls.Add(btn_chooseDir);
            grp_hashing_dir.Location = new System.Drawing.Point(8, 80);
            grp_hashing_dir.Name = "grp_hashing_dir";
            grp_hashing_dir.Size = new System.Drawing.Size(207, 134);
            grp_hashing_dir.TabIndex = 1;
            grp_hashing_dir.TabStop = false;
            grp_hashing_dir.Text = "Or";
            // 
            // menu_fileExtensions
            // 
            menu_fileExtensions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            menu_fileExtensions.FormattingEnabled = true;
            menu_fileExtensions.Location = new System.Drawing.Point(7, 88);
            menu_fileExtensions.Name = "menu_fileExtensions";
            menu_fileExtensions.Size = new System.Drawing.Size(191, 33);
            menu_fileExtensions.TabIndex = 3;
            // 
            // btn_chooseDir
            // 
            btn_chooseDir.Location = new System.Drawing.Point(7, 32);
            btn_chooseDir.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            btn_chooseDir.Name = "btn_chooseDir";
            btn_chooseDir.Size = new System.Drawing.Size(191, 48);
            btn_chooseDir.TabIndex = 2;
            btn_chooseDir.Text = "Choose a Directory";
            btn_chooseDir.UseVisualStyleBackColor = true;
            btn_chooseDir.Click += BtnChooseDir_Click;
            // 
            // btn_chooseFiles
            // 
            btn_chooseFiles.Location = new System.Drawing.Point(15, 32);
            btn_chooseFiles.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            btn_chooseFiles.Name = "btn_chooseFiles";
            btn_chooseFiles.Size = new System.Drawing.Size(191, 48);
            btn_chooseFiles.TabIndex = 0;
            btn_chooseFiles.Text = "Choose files";
            btn_chooseFiles.UseVisualStyleBackColor = true;
            btn_chooseFiles.Click += BtnChooseFiles_Click;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(txtStatus, 0, 1);
            tableLayoutPanel2.Controls.Add(groupFiles, 0, 0);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(233, 5);
            tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new System.Drawing.Size(954, 560);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // txtStatus
            // 
            txtStatus.BackColor = System.Drawing.Color.Black;
            txtStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            txtStatus.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtStatus.ForeColor = System.Drawing.Color.Lime;
            txtStatus.HideSelection = false;
            txtStatus.Location = new System.Drawing.Point(3, 283);
            txtStatus.Multiline = true;
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtStatus.Size = new System.Drawing.Size(948, 274);
            txtStatus.TabIndex = 12;
            // 
            // groupFiles
            // 
            groupFiles.Controls.Add(list_verification_results);
            groupFiles.Controls.Add(list_results);
            groupFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            groupFiles.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            groupFiles.Location = new System.Drawing.Point(3, 3);
            groupFiles.Name = "groupFiles";
            groupFiles.Size = new System.Drawing.Size(948, 274);
            groupFiles.TabIndex = 9;
            groupFiles.TabStop = false;
            // 
            // list_verification_results
            // 
            list_verification_results.AllowDrop = true;
            list_verification_results.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { col_results_verification_file, col_results_verification_isMatch });
            list_verification_results.Dock = System.Windows.Forms.DockStyle.Fill;
            list_verification_results.GridLines = true;
            list_verification_results.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            list_verification_results.HideSelection = false;
            list_verification_results.Location = new System.Drawing.Point(3, 27);
            list_verification_results.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            list_verification_results.Name = "list_verification_results";
            list_verification_results.Size = new System.Drawing.Size(942, 244);
            list_verification_results.SmallImageList = imgList_verification;
            list_verification_results.TabIndex = 11;
            list_verification_results.UseCompatibleStateImageBehavior = false;
            list_verification_results.View = System.Windows.Forms.View.Details;
            list_verification_results.DragDrop += list_results_DragDrop;
            list_verification_results.DragEnter += list_results_DragEnter;
            list_verification_results.Resize += List_verification_results_Resize;
            // 
            // col_results_verification_file
            // 
            col_results_verification_file.Text = "File";
            col_results_verification_file.Width = 200;
            // 
            // col_results_verification_isMatch
            // 
            col_results_verification_isMatch.Text = "Hash Match";
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
            // list_results
            // 
            list_results.AllowDrop = true;
            list_results.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHashResult_File, columnHashResult_Hash });
            list_results.Dock = System.Windows.Forms.DockStyle.Fill;
            list_results.FullRowSelect = true;
            list_results.GridLines = true;
            list_results.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            list_results.HideSelection = false;
            list_results.Location = new System.Drawing.Point(3, 27);
            list_results.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            list_results.Name = "list_results";
            list_results.Size = new System.Drawing.Size(942, 244);
            list_results.TabIndex = 10;
            list_results.UseCompatibleStateImageBehavior = false;
            list_results.View = System.Windows.Forms.View.Details;
            list_results.ItemSelectionChanged += List_results_ItemSelectionChanged;
            list_results.DragDrop += list_results_DragDrop;
            list_results.DragEnter += list_results_DragEnter;
            list_results.KeyDown += list_results_KeyDown;
            list_results.Resize += List_hashing_results_Resize;
            // 
            // columnHashResult_File
            // 
            columnHashResult_File.Text = "File";
            // 
            // columnHashResult_Hash
            // 
            columnHashResult_Hash.Text = "Hash";
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
            tableLayoutPanel1.ResumeLayout(false);
            group_Left.ResumeLayout(false);
            groupVerification.ResumeLayout(false);
            groupSettings.ResumeLayout(false);
            groupHashing.ResumeLayout(false);
            grp_hashing_dir.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            groupFiles.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btn_go;
        private System.Windows.Forms.ContextMenuStrip ctxMenu_results;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox group_Left;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ImageList imgList_verification;
        private System.Windows.Forms.GroupBox groupFiles;
        private System.Windows.Forms.TextBox txtStatus;
        private VerificationResultsList list_verification_results;
        private System.Windows.Forms.ColumnHeader col_results_verification_file;
        private System.Windows.Forms.ColumnHeader col_results_verification_isMatch;
        private FileHashResultList list_results;
        private System.Windows.Forms.ColumnHeader columnHashResult_File;
        private System.Windows.Forms.ColumnHeader columnHashResult_Hash;
        private System.Windows.Forms.GroupBox groupHashing;
        private System.Windows.Forms.Button btn_chooseFiles;
        private System.Windows.Forms.Button btn_chooseDir;
        private System.Windows.Forms.GroupBox groupVerification;
        private System.Windows.Forms.Button btn_openHashfile;
        private System.Windows.Forms.GroupBox grp_hashing_dir;
        private System.Windows.Forms.ComboBox menu_fileExtensions;
        private System.Windows.Forms.GroupBox groupSettings;
        private System.Windows.Forms.ComboBox menu_hashingAlgorithm;
        private System.Windows.Forms.ComboBox menu_decoderProfiles;
    }
}