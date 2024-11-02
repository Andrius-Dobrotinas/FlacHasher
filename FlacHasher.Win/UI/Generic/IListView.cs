﻿using System;
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
    }
}