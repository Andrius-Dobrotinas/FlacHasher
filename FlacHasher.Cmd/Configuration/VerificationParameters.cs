using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;
using static Andy.FlacHash.Cmd.Verification;

namespace Andy.FlacHash.Cmd
{
    public class VerificationParameters : IHashfileParams
    {
        [CmdLineParameter(ParameterNames.HashFile)]
        [AtLeastOneOf("hashfile")]
        [AtLeastOneOf("hashfileExtensions")]
        public string HashFile { get; set; }

        [IniEntry(nameof(HashfileExtensions))]
        [AtLeastOneOf("hashfileExtensions")]
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional(defaultValue: Defaults.HashfileEntrySeparator)]
        public string HashfileEntrySeparator { get; set; }

        [CmdLineParameter(ParameterNames.InputDirectory)]
        [AtLeastOneOf("hashfile")]
        public string InputDirectory { get; set; }

        public static class Defaults
        {
            public const string HashfileEntrySeparator = ":";
        }
    }
}