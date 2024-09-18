using Andy.Cmd.Parameter;
using Andy.FlacHash.Hashing.Crypto;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    // Optional is here to disallow empty strings
    public class ConfigurationProfile
    {
        [Parameter(nameof(Decoder))]
        [Optional]
        public string Decoder { get; set; }

        /// <summary>
        /// A semicolon-separated list of parameters to <see cref="Decoder"/> exactly the way they are supposed to appear
        /// (with dashes and whatnot)
        /// </summary>
        [Parameter(nameof(DecoderParameters))]
        [Optional]
        public string[] DecoderParameters { get; set; }

        [Parameter(nameof(TargetFileExtension))]
        [Optional]
        public string TargetFileExtension { get; set; }

        [Parameter(nameof(HashfileExtensions))]
        [Optional]
        public string[] HashfileExtensions { get; set; }

        [Parameter(nameof(HashfileEntrySeparator))]
        [Optional]
        public string HashfileEntrySeparator { get; set; }

        [Parameter(nameof(HashAlgorithm))]
        [Optional]
        public string HashAlgorithm { get; set; }

        [Parameter(nameof(FileLookupIncludeHidden))]
        public bool? FileLookupIncludeHidden { get; set; }

        [Parameter(nameof(OutputFormat))]
        [Optional]
        public string OutputFormat { get; set; }
        
        [Parameter(nameof(ProcessExitTimeoutMs))]
        public int? ProcessExitTimeoutMs { get; set; }

        [Parameter(nameof(ProcessTimeoutSec))]
        public int? ProcessTimeoutSec { get; set; }

        [Parameter(nameof(ProcessStartWaitMs))]
        public int? ProcessStartWaitMs { get; set; }

        [Parameter(nameof(FailOnError))]
        public bool? FailOnError { get; set; }

        public const string RootProfileAlias = ".";
        public static class Defaults
        {
            public static Algorithm HashAlgorithm = Algorithm.SHA256;
        }
    }

    public class Settings : ConfigurationProfile
    {
        /// <summary>
        /// When non-null, specifies override settings profile
        /// </summary>
        [Parameter(nameof(Profile))]
        [Optional]
        public string Profile { get; set; }
    }
}