using Andy.Cmd.Parameter;
using Andy.FlacHash.Hashfile.Read;
using System;
using System.Collections.Generic;
using static Andy.FlacHash.Application.Cmd.Verification;

namespace Andy.FlacHash.Application.Cmd
{
    public class VerificationParameters : MainParameters, IVerificationParams
    {
        [CmdLineParameter(CmdlineParameterNames.HashFile)]
        [AtLeastOneOf("hashfile")]
        public string HashFile { get; set; }


        [CmdLineParameter(CmdlineParameterNames.InputDirectory)]
        [AtLeastOneOf("hashfile")]
        [OptionalEitherOr("input")]
        public override string InputDirectory { get; set; }

        [CmdLineParameter(CmdlineParameterNames.InputFiles)]
        [OptionalEitherOr("input")]
        public string[] InputFiles { get; set; }

        [IniEntry(nameof(HashfileExtensions))]
        [AtLeastOneOf("hashfileExtensions")]
        [Optional(defaultValue: ApplicationSettings.Defaults.HashfileExtension)]
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional(defaultValue: HashFileReader.Default.HashfileEntrySeparator)]
        public string HashfileEntrySeparator { get; set; }

        [CmdLineParameter("--ignore-extra")]
        [IniEntry(nameof(InputIgnoreExtraneous))]
        [Optional(defaultValue: false)]
        public bool InputIgnoreExtraneous { get; set; }
    }
}