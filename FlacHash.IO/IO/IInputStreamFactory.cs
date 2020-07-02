using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO
{
    public interface IInputStreamFactory
    {
        Stream CreateStream(FileInfo sourceFile);
    }
}