using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.UI
{
    public interface IDisplayValueProducer<T>
    {
        string GetDisplayValue(T listItem);
    }
}