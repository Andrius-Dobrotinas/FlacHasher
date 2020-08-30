using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public class FileListBoxAdapter
    {
        private readonly ListBox list_files;

        public FileListBoxAdapter(ListBox list_files, string displayMember)
        {
            this.list_files = list_files;
            list_files.DisplayMember = displayMember;
        }

        public IEnumerable<FileInfo> GetItems()
        {
            return list_files.Items.Cast<FileInfo>();
        }

        public void ReplaceItems(FileInfo[] files)
        {
            list_files.Items.Clear();
            list_files.Items.AddRange(files);
        }
    }
}