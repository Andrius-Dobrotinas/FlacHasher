using Andy.Cmd;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.CompressionLevel
{
    public class ParameterReader
    {
        /// <summary>
        /// Properties of a returned object will only have Null values if the parameters are not specified. If a parameter is specified with no value, a corresponding property will be an empty string
        /// </summary>
        public static Parameters GetParameters(IDictionary<string, string> arguments)
        {
            var sourceFilePath = arguments
                .FirstOrDefault(x => x.Key.StartsWith(ArgumentNames.Prefix) == false)
                .Key;

            var compressioNlevel = GetValueOptionalAllowingEmpty(arguments, $"{ArgumentNames.Prefix}{ArgumentNames.CompressionLevel}");

            return new Parameters
            {
                SourceFile = sourceFilePath,
                FlacExec = GetValueOptionalAllowingEmpty(arguments, $"{ArgumentNames.Prefix}{ArgumentNames.FlacExec}"),
                CompressionLevel = compressioNlevel == null ? (int?)null : int.Parse(compressioNlevel)
            };
        }

        /// <summary>
        /// If the argument is present, returns its value regardless whether it actually has one (eg empty string).
        /// If it happens not to have a value, returns <paramref name="valueIfEmpty"/> value or an empty string.
        /// If the argument is not present, returns null.
        /// </summary>
        static string GetValueOptionalAllowingEmpty(IDictionary<string, string> arguments, string argName, string valueIfEmpty = null)
        {
            string value;

            if (!arguments.TryGetValue(argName, out value))
                return null;

            return string.IsNullOrEmpty(value)
                ? valueIfEmpty ?? ""
                : value;
        }
    }
}