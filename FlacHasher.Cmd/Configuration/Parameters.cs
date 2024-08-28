using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public class Parameters
    {
        [Parameter(ParameterNames.Profile)]
        [Optional]
        public string Profile { get; set; }

        [Parameter(ParameterNames.Decoder)]
        [Optional]
        public string Decoder { get; set; }

        [Parameter(ParameterNames.HashAlgorithm)]
        [Optional]
        public string HashAlgorithm { get; set; }
        
        [Parameter(ParameterNames.InputFiles)]
        [EitherOr("input")]
        public string[] InputFiles { get; set; }
        
        [Parameter(ParameterNames.InputDirectory)]
        [EitherOr("input")]
        public string InputDirectory { get; set; }

        [Parameter(ParameterNames.FileExtension)]
        [Optional]
        public string TargetFileExtension { get; set; }

        [Parameter(ParameterNames.OutputFormat)]
        [Optional]
        public string OutputFormat { get; set; }

        [Parameter(ParameterNames.ProcessExitTimeoutMs)]
        public int? ProcessExitTimeoutMs { get; set; }

        [Parameter(ParameterNames.ProcessTimeoutSec)]
        public int? ProcessTimeoutSec { get; set; }
        
        [Parameter(ParameterNames.FailOnError)]
        public bool? FailOnError { get; set; }
        
        [Parameter(ParameterNames.HashFile)]
        [Optional]
        public string HashFile { get; set; }
    }
}