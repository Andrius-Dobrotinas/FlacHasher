using System;
using System.Collections.Generic;

namespace Andy.FlacHash.IO
{
    public interface IFileReadEventSource
    {
        event BytesReadHandler BytesRead;
    }
}