using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Andy.FlacHash.Hashfile.Read
{
    public class CaseInsensitiveOrdinalStringComparer : IEqualityComparer<string>
    {
        public bool Equals([AllowNull] string x, [AllowNull] string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            throw new NotImplementedException();
        }
    }
}