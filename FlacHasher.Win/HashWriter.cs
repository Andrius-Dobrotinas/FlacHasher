using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    public class HashWriter
    {
        private readonly SaveFileDialog saveFileDialog;

        public HashWriter(SaveFileDialog saveFileDialog)
        {
            this.saveFileDialog = saveFileDialog;
        }

        public bool SaveHashes(IEnumerable<string> hashes)
        {
            var result = saveFileDialog.ShowDialog();
            if (result != DialogResult.OK) return false;

            IOUtil.WriteToFile(new System.IO.FileInfo(saveFileDialog.FileName), hashes);

            return true;
        }
    }
}