using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public interface IFileList
    {
        IEnumerable<FileInfo> GetItems();
        void ReplaceItems(FileInfo[] files);
    }

    public class FileList : ListBox, IFileList
    {
        public FileList()
        {
            this.DisplayMember = nameof(FileInfo.Name);
        }

        public IEnumerable<FileInfo> GetItems()
        {
            return this.Items.Cast<FileInfo>();
        }

        public void ReplaceItems(FileInfo[] files)
        {
            this.Items.Clear();
            this.Items.AddRange(files);
        }
    }
}