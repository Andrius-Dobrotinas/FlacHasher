using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.UI
{
    public interface IListItem<TValue>
    {
        TValue Value { get; set; }
        string DisplayValue { get; set; }
    }
}