using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO
{
    public class ReadStreamFactory : IInputStreamFactory
    {
        public Stream CreateStream(FileInfo sourceFile)
        {
            if (sourceFile.Exists == false)
                throw new FileNotFoundException($"File not found: {sourceFile.FullName}", sourceFile.FullName);
            
            var stream = new FileStream(sourceFile.FullName, FileMode.Open, FileAccess.Read);

            return stream;
        }
    }
}