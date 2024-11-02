using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public interface IFileResultListView : IListView<FileInfo>
    {
    }

    public class FileResultListView : ListView, IFileResultListView
    {
        protected IEnumerable<ListViewItem> ListViewItems => this.Items.Cast<ListViewItem>();

        protected ListViewItem FindItem(FileInfo key)
        {
            return ListViewItems.FirstOrDefault(x => x.Name == key.FullName)
                ?? throw new Exception($"Item not found: {key.FullName}");
        }

        public void AddRange(FileInfo[] files)
        {
            var items = files.Select(
                file => new ListViewItem
                {
                    Name = file.FullName,
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

        public IEnumerator<ListViewItem> GetEnumerator()
        {
            return ListViewItems.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ListViewItems.GetEnumerator();
        }
    }
}