using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public interface IFileListView : IListView<FileInfo, ListViewItem<FileInfo>>
    {
    }

    public abstract class FileResultListView<TData> : ListView, IFileListView
    {
        protected IEnumerable<ListViewItem<FileInfo>> ListViewItems => this.Items.Cast<ListViewItem<FileInfo>>();

        protected ListViewItem<FileInfo> FindItem(FileInfo key)
        {
            return ListViewItems.FirstOrDefault(x => x.Key == key)
                ?? throw new Exception($"Item not found: {key.FullName}");
        }

        public void UpdateItem(FileInfo key, TData data)
        {
            var item = FindItem(key);

            UpdateItem(item, data);
        }

        protected abstract void UpdateItem(ListViewItem<FileInfo> item, TData data);

        public void AddRange(FileInfo[] files)
        {
            var items = files.Select(
                file => new ListViewItem<FileInfo>
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

        public IEnumerator<ListViewItem<FileInfo>> GetEnumerator()
        {
            return ListViewItems.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ListViewItems.GetEnumerator();
        }
    }
}