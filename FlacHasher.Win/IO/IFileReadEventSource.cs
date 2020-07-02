using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win.IO
{
    public interface IFileReadEventSource
    {
        event BytesReadHandler BytesRead;
    }
}