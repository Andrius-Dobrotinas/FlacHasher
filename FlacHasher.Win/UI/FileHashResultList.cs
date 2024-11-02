using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Windows.Forms.ListViewItem;

namespace Andy.FlacHash.Application.Win.UI
{
    public class FileHashResultListItem
    {
        public FileInfo File { get; set; }
        public string HashString { get; set; }
    }

    public class FileHashResultList : FileResultListView<FileHashResultListItem>
    {
        protected override void UpdateItem(ListViewItem<FileInfo> item, FileHashResultListItem data)
        {
            item.SubItems.Add(new ListViewSubItem
            {
                Text = data.HashString
            });
        }
    }
}