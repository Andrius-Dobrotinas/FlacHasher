using Andy.Cmd.Parameter;
using Andy.FlacHash.Crypto;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application
{
    public class ApplicationSettings
    {
        [CmdLineParameter(CmdlineParameterNames.Decoder, Order = 0)]
        [IniEntry(nameof(Decoder), Order = 1)]
        public string Decoder { get; set; }

        /// <summary>
        /// An array of parameters to <see cref="Decoder"/> exactly the way they are supposed to appear
        /// (with dashes and whatnot).
        /// There are default parameters defined in the code
        /// </summary>
        [IniEntry(nameof(DecoderParameters))]
        [Optional]
        public string[] DecoderParameters { get; set; }

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

        public static class Defaults
        {
            public const Algorithm HashAlgorithm = Algorithm.SHA256;
            public const string HashfileExtension = "hash";
        }
    }
}