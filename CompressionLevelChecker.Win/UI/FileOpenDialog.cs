using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public class FileOpenDialog
    {
        private readonly OpenFileDialog dialog;

        public FileOpenDialog(OpenFileDialog dialog)
        {
            this.dialog = dialog;
        }

        public System.IO.FileInfo GetFile()
        {
            if (dialog.ShowDialog() == DialogResult.OK)
                return new System.IO.FileInfo(dialog.FileName);
            else
                return null;
        }
    }
}