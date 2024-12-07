using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Andy.FlacHash.Verification
{
    public static class HashMatchValueFormatter
    {
        private static readonly Regex regex = new Regex(@"([a-z])([A-Z])");

        public static string GetString(HashMatch value)
        {
            return regex.Replace(value.ToString(), "$1 $2");
        }
    }
}
