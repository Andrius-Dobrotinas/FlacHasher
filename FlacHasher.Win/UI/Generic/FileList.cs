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
        bool Any();
    }

    public class FileList : ListBox, IFileList
    {
        public FileList()
        {
            this.DisplayMember = nameof(FileInfo.Name);
        }

        public bool Any()
        {
            return this.Items.Count > 0;
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