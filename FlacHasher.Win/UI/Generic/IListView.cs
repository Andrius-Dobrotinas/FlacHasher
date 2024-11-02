using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public interface IListView<TItem> : IEnumerable<ListViewItem>
    {
        void AddRange(TItem[] files);
        void ClearList();
        void Reset(params TItem[] files);
    }
}