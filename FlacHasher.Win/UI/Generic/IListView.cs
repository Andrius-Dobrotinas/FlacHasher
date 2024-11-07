using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Win.UI
{
    public interface IListView<TKey, TListViewItem> : IEnumerable<TListViewItem>
        where TListViewItem : ListViewItem<TKey>
    {
        IEnumerable<TKey> ItemKeys { get; }
        void AddRange(TKey[] itemKey);
        void ClearList();

        /// <summary>
        /// Clears the list and Adds new items
        /// </summary>
        /// <param name="newItemKeys"></param>
        void Reset(params TKey[] newItemKeys);

        /// <summary>
        /// Removes all data retaining actual elements
        /// </summary>
        void ResetData();
    }
}