using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.UI
{
    public class ListItem<T> : IListItem<T>
    {
        public T Value { get; set; }
        public string DisplayValue { get; set; }
    }
}