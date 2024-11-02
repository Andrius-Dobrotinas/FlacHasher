using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public interface IListView<TData, TListItem> : IEnumerable<TListItem>
        where TListItem : ListViewItem
    {
        void AddRange(TData[] itemKey);
        void ClearList();

        /// <summary>
        /// Clears the list and Adds new items
        /// </summary>
        /// <param name="newItemKeys"></param>
        void Reset(params TData[] newItemKeys);
    }
}