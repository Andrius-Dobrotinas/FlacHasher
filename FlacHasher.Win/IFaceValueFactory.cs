using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win
{
    public interface IFaceValueFactory<T>
    {
        string GetFaceValue(T listItem);
    }
}