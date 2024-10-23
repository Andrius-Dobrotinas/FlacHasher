using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;
using static Andy.FlacHash.Application.Cmd.Verification;

namespace Andy.FlacHash.Application.Cmd
{
    public class VerificationParameters : IHashfileParams
    {
        [CmdLineParameter(CmdlineParameterNames.HashFile)]
        [AtLeastOneOf("hashfile")]
        [AtLeastOneOf("hashfileExtensions")]
        public string HashFile { get; set; }

        [IniEntry(nameof(HashfileExtensions))]
        [AtLeastOneOf("hashfileExtensions")]
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional(defaultValue: FlacHash.Hashing.Verification.HashFileReader.Default.HashfileEntrySeparator)]
        public string HashfileEntrySeparator { get; set; }

        [CmdLineParameter(CmdlineParameterNames.InputDirectory)]
        [AtLeastOneOf("hashfile")]
        public string InputDirectory { get; set; }
    }
}