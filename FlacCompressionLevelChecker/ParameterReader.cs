using Andy.Cmd;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash
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

            var compressioNlevel = Parameter.GetValue(arguments, $"{ArgumentNames.Prefix}{ArgumentNames.CompressionLevel}");

            return new Parameters
            {
                SourceFile = sourceFilePath,
                FlacExec = Parameter.GetValue(arguments, $"{ArgumentNames.Prefix}{ArgumentNames.FlacExec}"),
                CompressionLevel = compressioNlevel == null ? (uint?)null : uint.Parse(compressioNlevel)
            };
        }        
    }
}