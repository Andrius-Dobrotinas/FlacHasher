using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash
{
    public class FileInfoEqualityComprarer : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo x, FileInfo y)
        {
            return string.Equals(x.FullName, y.FullName, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(FileInfo obj)
        {
            return obj.FullName.ToLowerInvariant().GetHashCode();
        }
    }
}