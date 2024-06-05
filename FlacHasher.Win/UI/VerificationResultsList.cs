using Andy.FlacHash.Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public class VerificationResultsList : ListView
    {
        public void Add(FileInfo file, bool isMatch)
        {
            var item = new ListViewItem
            {
                Text = file.Name == HashEntryMatching.MissingFileKey ? "File's missing" : file.Name,
                ImageIndex = isMatch ? 0 : 1,
            };

            item.SubItems.Add(isMatch.ToString());

            this.Items.Add(item);
        }
    }
}