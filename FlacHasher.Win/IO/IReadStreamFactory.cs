using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win.IO
{
    public interface IReadStreamFactory
    {
        Stream CreateStream(FileInfo sourceFile);
    }
}