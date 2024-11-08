using Andy.FlacHash.Verification;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Application.Win.UI
{
    public class VerificationResultsList : FileResultListView<HashMatch>
    {
        protected override void UpdateItem(ListViewItem<FileInfo> item, HashMatch isMatch)
        {
            item.ImageIndex = (int)(isMatch == HashMatch.NotFound ? HashMatch.Error : isMatch);
            item.SubItems.Add(isMatch.ToString());
        }
    }
}