using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Win
{
    internal class Util
    {
        public static int FindIndex<T>(IList<T> source, Func<T, bool> isMatch)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (isMatch(source[i]))
                    return i;
            }

            return -1;
        }
    }
}
