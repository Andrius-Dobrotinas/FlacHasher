using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Windows.Forms.ListViewItem;

namespace Andy.FlacHash.Application.Win.UI
{
    public class FileHashResultListItem
    {
        public string FileName { get; set; }
        public string HashString { get; set; }
    }

    public class FileHashResultList : FileResultListView
    {
        const string SubitemHashKey = "hash";

        public void UpdateItem(FileInfo file, string hashstring)
        {
            var item = FindItem(file);

            item.SubItems.Add(new ListViewSubItem
            {
                Name = SubitemHashKey,
                Text = hashstring
            });
        }

        public IEnumerable<FileHashResultListItem> GetUnderlyingData()
        {
            return ListViewItems.Select(x => new FileHashResultListItem
            {
                FileName = x.Name,
                HashString = x.SubItems.Cast<ListViewSubItem>().FirstOrDefault(x => x.Name == SubitemHashKey)?.Text
            });
        }
    }
}