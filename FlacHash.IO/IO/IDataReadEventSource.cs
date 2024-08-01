using System;
using System.Collections.Generic;

namespace Andy.FlacHash.IO
{
    public interface IDataReadEventSource
    {
        event BytesReadHandler BytesRead;
    }
}