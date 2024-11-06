using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public interface IFileListView : IListView<FileInfo>
    {
    }

    public interface IFileListView<TData> : IFileListView
    {
        public void SetData(FileInfo key, TData data);
        public IEnumerable<KeyValuePair<FileInfo, TData>> BackingData { get; }
    }

    public abstract class FileResultListView<TData> : ListView, IFileListView<TData>
    {
        public event EventHandler<IEnumerable<FileInfo>> ItemsAdded;
        public event EventHandler Cleared;

        protected IEnumerable<ListViewItem<FileInfo, TData>> ListViewItems
            => this.Items.Cast<ListViewItem<FileInfo, TData>>();

        public IEnumerable<KeyValuePair<FileInfo, TData>> BackingData
            => this.ListViewItems.Select(x => new KeyValuePair<FileInfo, TData>(x.Key, x.Data));

        protected ListViewItem<FileInfo, TData> FindItem(FileInfo key)
        {
            return ListViewItems.FirstOrDefault(x => x.Key == key)
                ?? throw new Exception($"Item not found: {key.FullName}");
        }

        public void SetData(FileInfo key, TData data)
        {
            var item = FindItem(key);
            item.Data = data;

            UpdateItem(item, data);
        }

        protected abstract void UpdateItem(ListViewItem<FileInfo> item, TData data);

        public void AddRange(FileInfo[] files)
        {
            var items = files.Select(
                file => new ListViewItem<FileInfo, TData>
                {
                    Key = file,
                    Text = file.Name,
                });

            this.Items.AddRange(items.ToArray());

            ItemsAdded?.Invoke(this, files);
        }

        public void ClearList()
        {
            this.Items.Clear();
            Cleared?.Invoke(this, null);
        }

        public void Reset(FileInfo[] files)
        {
            ClearList();
            AddRange(files);
        }

        public void ResetData()
        {
            Reset(this.ToArray());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ListViewItems.Select(x => x.Key).GetEnumerator();
        }

        IEnumerator<FileInfo> IEnumerable<FileInfo>.GetEnumerator()
        {
            return ListViewItems.Select(x => x.Key).GetEnumerator();
        }
    }
}