using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Andy.FlacHash.Verification
{
    public static class HashMatchValueFormatter
    {
        public static string GetString(HashMatch value)
        {
            return Regex.Replace(value.ToString(), @"([a-z])([A-Z])", "$1 $2");
        }
    }
}
