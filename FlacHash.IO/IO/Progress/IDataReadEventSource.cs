using System;
using System.Collections.Generic;

namespace Andy.FlacHash.IO.Progress
{
    public interface IDataReadEventSource
    {
        event BytesReadHandler BytesRead;
    }
}