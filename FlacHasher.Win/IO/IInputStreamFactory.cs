using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win.IO
{
    public interface IInputStreamFactory
    {
        Stream CreateStream(FileInfo sourceFile);
    }
}