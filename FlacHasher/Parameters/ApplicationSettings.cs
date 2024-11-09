using Andy.Cmd.Parameter;
using Andy.FlacHash.Crypto;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application
{
    public class ApplicationSettings
    {
        [CmdLineParameter(CmdlineParameterNames.ProcessExitTimeoutMs, Order = 0)]
        [IniEntry(nameof(ProcessExitTimeoutMs), Order = 1)]
        [Optional(defaultValue: 1000)]
        public int ProcessExitTimeoutMs { get; set; }

        // TODO: document: -1 for no timeout
        [CmdLineParameter(CmdlineParameterNames.ProcessTimeoutSec, Order = 0)]
        [IniEntry(nameof(ProcessTimeoutSec), Order = 1)]
        [Optional(defaultValue: 180)]
        public int ProcessTimeoutSec { get; set; }

        [IniEntry(nameof(ProcessStartWaitMs))]
        [Optional(defaultValue: 100)]
        public int ProcessStartWaitMs { get; set; }

        [CmdLineParameter(CmdlineParameterNames.HashAlgorithm, Order = 0)]
        [IniEntry(nameof(HashAlgorithm), Order = 1)]
        [Optional(defaultValue: Defaults.HashAlgorithm)]
        public Algorithm HashAlgorithm { get; set; }

        [IniEntry(nameof(FileLookupIncludeHidden))]
        [Optional(defaultValue: false)]
        public bool FileLookupIncludeHidden { get; set; }

        public const string ProfileKey = "Profile";
        public const string DecoderProfileKey = "Profile.Decoder";
        public const string HashingProfileKey = "Profile.Hashing";
        public const string DecoderSectionPrefix = "Decoder";
        public const string DefaultDecoderSection = "FLAC";
        public const string HashingSectionPrefix = "Hashing";
        public const string DefaultHashingSection = "";

        public static class Defaults
        {
            public const Algorithm HashAlgorithm = Algorithm.SHA256;
            public const string HashfileExtension = "hash";
        }
    }
}