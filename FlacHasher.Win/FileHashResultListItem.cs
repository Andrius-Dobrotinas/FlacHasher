using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win
{
    public class FileHashResultListItem<T> : IListItem
    {
        public T Result { get; set; }
        public string FaceValue { get; set; }
    }
}