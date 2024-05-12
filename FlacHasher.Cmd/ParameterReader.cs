using Andy.Cmd;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public class ParameterReader
    {
        /// <summary>
        /// Properties of a returned object will only have Null values if the parameters are not specified. If a parameter is specified with no value, a corresponding property will be an empty string
        /// </summary>
        public static Parameters GetParameters(IDictionary<string, string> arguments)
        {
            return new Parameters
            {
                Decoder = Parameter.GetValue(arguments, ArgumentNames.Decoder),
                InputFiles = Parameter.GetValue<string[]>(arguments, ArgumentNames.InputFiles, paths => paths.Split(';')),
                InputDirectory = Parameter.GetValue(arguments, ArgumentNames.InputDirectory),
                TargetFileExtension = Parameter.GetValue(arguments, ArgumentNames.FileExtension),
                OutputFormat = Parameter.GetValue(arguments, ArgumentNames.OutputFormat),
                ProcessExitTimeoutMs = Parameter.GetValue<int?>(arguments, ArgumentNames.ProcessExitTimeoutMs, value => int.Parse(value))
            };
        }
    }
}