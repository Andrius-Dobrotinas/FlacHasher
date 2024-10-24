using System;
using System.Collections.Generic;

namespace Andy.IO
{
    public interface IDataReadEventSource
    {
        event BytesReadHandler BytesRead;
    }
}