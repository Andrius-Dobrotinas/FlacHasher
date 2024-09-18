using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Cmd
{
    public class ApplicationSettings
    {
        public string Decoder { get; set; }

        /// <summary>
        /// There are default parameters defined in the code
        /// </summary>
        [Optional]
        public string[] DecoderParameters { get; set; }

        public int? ProcessExitTimeoutMs { get; set; }
        public int? ProcessTimeoutSec { get; set; }
        public int? ProcessStartWaitMs { get; set; }

        // TODO: a default value is used when this is empty. Should I change this to Enum type?
        [Optional]
        public string HashAlgorithm { get; set; }

        public bool? FailOnError { get; set; }

        [Optional]
        public string OutputFormat { get; set; }

        [EitherOr("input")]
        public string[] InputFiles { get; set; }

        [EitherOr("input")]
        public string InputDirectory { get; set; }

        [RequiredWith(nameof(InputDirectory))]
        public string TargetFileExtension { get; set; }

        public bool FileLookupIncludeHidden { get; set; }


        [Parameter(ParameterNames.ModeVerify)]
        public bool? IsVerification { get; set; }

        [Optional]
        public string HashFile { get; set; }

        [Optional]
        public string[] HashfileExtensions { get; set; }

        [Optional]
        public string HashfileEntrySeparator { get; set; }

        [Optional]
        public string Profile { get; set; }
    }
}