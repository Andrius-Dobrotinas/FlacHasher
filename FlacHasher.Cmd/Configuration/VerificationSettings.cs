﻿using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;
using static Andy.FlacHash.Cmd.Verification;

namespace Andy.FlacHash.Cmd
{
    public class VerificationSettings : IHashfileParams
    {
        [CmdLineParameter(ParameterNames.HashFile)]
        [Optional]
        public string HashFile { get; set; }

        [IniEntry(nameof(HashfileExtensions))]
        [Optional]
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional]
        public string HashfileEntrySeparator { get; set; }

        [CmdLineParameter(ParameterNames.InputDirectory)]
        public string InputDirectory { get; set; }
    }
}