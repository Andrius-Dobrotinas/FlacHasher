using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public interface IFileResultListView : IListView<FileInfo, FileListViewItem>
    {
    }

    public class FileListViewItem : ListViewItem
    {
        public FileInfo Key { get; set; }
    }

    public class FileResultListView : ListView, IFileResultListView
    {
        protected IEnumerable<FileListViewItem> ListViewItems => this.Items.Cast<FileListViewItem>();

        protected FileListViewItem FindItem(FileInfo key)
        {
            return ListViewItems.FirstOrDefault(x => x.Key == key)
                ?? throw new Exception($"Item not found: {key.FullName}");
        }

        public void AddRange(FileInfo[] files)
        {
            var items = files.Select(
                file => new FileListViewItem
                {
                    Key = file,
                    Text = file.Name,
                });

            this.Items.AddRange(items.ToArray());
        }

        public void ClearList()
        {
            this.Items.Clear();
        }

        public void Reset(FileInfo[] files)
        {
            ClearList();
            AddRange(files);
        }

        public IEnumerator<FileListViewItem> GetEnumerator()
        {
            return ListViewItems.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ListViewItems.GetEnumerator();
        }
    }
}