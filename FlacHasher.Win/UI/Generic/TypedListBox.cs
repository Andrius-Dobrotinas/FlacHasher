using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public interface ITypedListBox<TListItem> where TListItem : class
    {
        void AddItem(TListItem result);
        void ClearList();
    }

    /// <summary>
    /// A strongly-typed wrapper of <see cref="ListBox"/>
    /// </summary>
    public class TypedListBox<TListItem> : ListBox, ITypedListBox<TListItem>
         where TListItem : class
    {
        public void AddItem(TListItem value)
        {
            this.Items.Add(value);
        }

        public void ClearList()
        {
            this.Items.Clear();
        }
    }
}