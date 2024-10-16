using System;
using System.Collections.Generic;
using System.IO;
using Andy.FlacHash.Audio;

namespace Andy.FlacHash.IO
{
    public class ReadStreamFactory : IFileReadStreamFactory
    {
        public Stream CreateStream(FileInfo sourceFile)
        {
            if (sourceFile.Exists == false)
                throw new InputFileNotFoundException(sourceFile.FullName);
            
            var stream = new FileStream(sourceFile.FullName, FileMode.Open, FileAccess.Read);

            return stream;
        }
    }
}