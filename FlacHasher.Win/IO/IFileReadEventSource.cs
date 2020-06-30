using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.IO
{
    public interface IFileReadProgressWatcher
    {
        event BytesReadHandler BytesRead;
    }
}