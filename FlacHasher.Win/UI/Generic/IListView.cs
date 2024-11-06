using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Win.UI
{
    public interface IListView<TKey> : IEnumerable<TKey>
    {
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

        event EventHandler<IEnumerable<TKey>> ItemsAdded;
        event EventHandler Cleared;
    }
}