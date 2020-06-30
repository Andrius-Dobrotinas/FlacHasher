using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win
{
    public class ListItem<T> : IListItem<T>
    {
        public T Value { get; set; }
        public string FaceValue { get; set; }
    }
}