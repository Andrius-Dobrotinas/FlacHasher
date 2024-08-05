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
                Profile = Parameter.TryGetValueAllowingEmpty(arguments, ParameterNames.Profile, "."),
                Decoder = Parameter.GetValueOptional(arguments, ParameterNames.Decoder),
                InputFiles = Parameter.GetValueOptional<string[]>(arguments, ParameterNames.InputFiles, paths => paths.Split(';')),
                InputDirectory = Parameter.GetValueOptional(arguments, ParameterNames.InputDirectory),
                TargetFileExtension = Parameter.GetValueOptional(arguments, ParameterNames.FileExtension),
                HashAlgorithm = Parameter.GetValueOptional(arguments, ParameterNames.HashAlgorithm),
                HashFile = Parameter.GetValueOptional(arguments, ParameterNames.HashFile),
                OutputFormat = Parameter.GetValueOptional(arguments, ParameterNames.OutputFormat),
                ProcessExitTimeoutMs = Parameter.GetValueOptional<int?>(arguments, ParameterNames.ProcessExitTimeoutMs, value => int.Parse(value)),
                ProcessTimeoutSec = Parameter.GetValueOptional<int?>(arguments, ParameterNames.ProcessTimeoutSec, value => int.Parse(value)),
                FailOnError = Parameter.GetBoolValue(arguments, ParameterNames.FailOnError)
            };
        }
    }
}