using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.UI
{
    public interface IFaceValueFactory<T>
    {
        string GetFaceValue(T listItem);
    }
}