﻿using Andy.Cmd.Parameter;
using Andy.FlacHash.Crypto;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application
{
    public class ApplicationSettings
    {
        [DecoderParam]
        [CmdLineParameter(CmdlineParameterNames.ProcessExitTimeoutMs, Order = 0)]
        [IniEntry(nameof(ProcessExitTimeoutMs), Order = 1)]
        [Optional(defaultValue: 1000)]
        public int ProcessExitTimeoutMs { get; set; }

        // TODO: document: -1 for no timeout
        [DecoderParam]
        [CmdLineParameter(CmdlineParameterNames.ProcessTimeoutSec, Order = 0)]
        [IniEntry(nameof(ProcessTimeoutSec), Order = 1)]
        [Optional(defaultValue: 180)]
        [ParameterDescription("The maximum amount of time (in seconds) an Audio decoder is allowed to take to decode a file. This is useful for situations where something goes wrong and the decoder stops responding and needs to be murdered")]
        public int ProcessTimeoutSec { get; set; }

        [DecoderParam]
        [IniEntry(nameof(ProcessStartWaitMs))]
        [Optional(defaultValue: 100)]
        public int ProcessStartWaitMs { get; set; }

        [OperationParam]
        [CmdLineParameter(CmdlineParameterNames.HashAlgorithm, Order = 0)]
        [IniEntry(nameof(HashAlgorithm), Order = 1)]
        [Optional(defaultValue: Defaults.HashAlgorithm)]
        [ParameterDescription($"Available options: {nameof(Algorithm.MD5)}, {nameof(Algorithm.SHA1)}, {nameof(Algorithm.SHA256)}, {nameof(Algorithm.SHA512)}")]
        [FrontAndCenterParam]
        public Algorithm HashAlgorithm { get; set; }

        [CmdLineParameter(CmdlineParameterNames.FileLookupIncludeHidden, Order = 0)]
        [IniEntry(nameof(FileLookupIncludeHidden), Order = 1)]
        [Optional(defaultValue: false)]
        [ParameterDescription("Whether to include hidden files when scanning a directory")]
        public bool FileLookupIncludeHidden { get; set; }

        public const string ProfileKey = "Profile";
        public const string DecoderProfileKey = "Profile.Decoder";
        public const string HashingProfileKey = "Profile.Hashing";
        public const string DecoderSectionPrefix = "Decoder";
        public const string DefaultDecoderSection = "";
        public const string HashingSectionPrefix = "Hashing";
        public const string DefaultHashingSection = "";

        public static class Defaults
        {
            public const Algorithm HashAlgorithm = Algorithm.MD5;
            public const string HashfileExtension = "hash";
        }
    }
}