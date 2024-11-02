using Andy.FlacHash.Verification;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Application.Win.UI
{
    public class VerificationResultsList : FileResultListView
    {
        public void UpdateItem(FileInfo file, HashMatch isMatch)
        {
            var item = FindItem(file);

            item.ImageIndex = (int)(isMatch == HashMatch.NotFound ? HashMatch.Error : isMatch);
            item.SubItems.Add(isMatch.ToString());
        }
    }
}