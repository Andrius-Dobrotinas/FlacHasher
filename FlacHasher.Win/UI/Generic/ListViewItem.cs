using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    public class ListViewItem<TKey> : ListViewItem
    {
        public TKey Key { get; set; }
    }
}