using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    public class InteractiveTextFileWriter
    {
        private readonly SaveFileDialog saveFileDialog;

        public InteractiveTextFileWriter(SaveFileDialog saveFileDialog)
        {
            this.saveFileDialog = saveFileDialog;
        }

        /// <summary>
        /// Asks a user where which file to write data to and then writes it
        /// </summary>
        public bool GetFileAndSave(IEnumerable<string> hashes)
        {
            var result = saveFileDialog.ShowDialog();
            if (result != DialogResult.OK) return false;

            IOUtil.WriteToFile(new System.IO.FileInfo(saveFileDialog.FileName), hashes);

            return true;
        }
    }
}