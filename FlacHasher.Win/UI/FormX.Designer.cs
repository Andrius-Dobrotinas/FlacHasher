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
            this.list_files = new FileList();
            this.btn_go = new System.Windows.Forms.Button();
            this.list_results = new FileHashResultList();
            this.ctxMenu_results = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btn_chooseDir
            // 
            this.btn_chooseDir.Location = new System.Drawing.Point(12, 9);
            this.btn_chooseDir.Name = "btn_chooseDir";
            this.btn_chooseDir.Size = new System.Drawing.Size(119, 25);
            this.btn_chooseDir.TabIndex = 0;
            this.btn_chooseDir.Text = "Choose a dir";
            this.btn_chooseDir.UseVisualStyleBackColor = true;
            this.btn_chooseDir.Click += new System.EventHandler(this.BtnChooseDir_Click);
            // 
            // list_files
            // 
            this.list_files.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.list_files.FormattingEnabled = true;
            this.list_files.Location = new System.Drawing.Point(137, 9);
            this.list_files.Name = "list_files";
            this.list_files.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.list_files.Size = new System.Drawing.Size(566, 147);
            this.list_files.TabIndex = 2;
            // 
            // btn_go
            // 
            this.btn_go.Location = new System.Drawing.Point(15, 124);
            this.btn_go.Name = "btn_go";
            this.btn_go.Size = new System.Drawing.Size(115, 31);
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
            this.list_results.FormattingEnabled = true;
            this.list_results.Location = new System.Drawing.Point(137, 162);
            this.list_results.Name = "list_results";
            this.list_results.Size = new System.Drawing.Size(566, 121);
            this.list_results.TabIndex = 4;
            // 
            // ctxMenu_results
            // 
            this.ctxMenu_results.Name = "ctxMenu_results";
            this.ctxMenu_results.Size = new System.Drawing.Size(61, 4);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(15, 162);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(115, 22);
            this.progressBar.TabIndex = 5;
            // 
            // FormX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 296);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.list_results);
            this.Controls.Add(this.btn_go);
            this.Controls.Add(this.list_files);
            this.Controls.Add(this.btn_chooseDir);
            this.Name = "FormX";
            this.Text = "FormX";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_chooseDir;
        private FileList list_files;
        private System.Windows.Forms.Button btn_go;
        private FileHashResultList list_results;
        private System.Windows.Forms.ContextMenuStrip ctxMenu_results;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}