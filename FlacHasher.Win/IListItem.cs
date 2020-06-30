using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win
{
    public interface IListItem<TValue>
    {
        TValue Value { get; set; }
        string FaceValue { get; set; }
    }
}