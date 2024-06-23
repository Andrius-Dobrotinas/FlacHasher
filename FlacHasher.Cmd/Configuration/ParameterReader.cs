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
                Decoder = Parameter.GetValueOptional(arguments, ArgumentNames.Decoder),
                InputFiles = Parameter.GetValueOptional<string[]>(arguments, ArgumentNames.InputFiles, paths => paths.Split(';')),
                InputDirectory = Parameter.GetValueOptional(arguments, ArgumentNames.InputDirectory),
                TargetFileExtension = Parameter.GetValueOptional(arguments, ArgumentNames.FileExtension),
                HashFile = Parameter.GetValueOptional(arguments, ArgumentNames.HashFile),
                OutputFormat = Parameter.GetValueOptional(arguments, ArgumentNames.OutputFormat),
                ProcessExitTimeoutMs = Parameter.GetValueOptional<int?>(arguments, ArgumentNames.ProcessExitTimeoutMs, value => int.Parse(value)),
                ProcessTimeoutSec = Parameter.GetValueOptional<int?>(arguments, ArgumentNames.ProcessTimeoutSec, value => int.Parse(value)),
                FailOnError = Parameter.GetBoolValue(arguments, ArgumentNames.FailOnError)
            };
        }
    }
}