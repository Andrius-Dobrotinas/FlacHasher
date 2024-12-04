using Andy.Cmd.Parameter;
using Andy.FlacHash.Hashfile.Read;
using System;
using System.Collections.Generic;
using static Andy.FlacHash.Application.Cmd.Verification;

namespace Andy.FlacHash.Application.Cmd
{
    public class VerificationParameters : MasterParameters, IVerificationParams
    {
        [OperationParam]
        [CmdLineParameter(CmdlineParameterNames.HashFile)]
        [AtLeastOneOf("hashfile")]
        [ParameterDescription($"Path to the hashfile. If relative, {nameof(InputDirectory)} is required")]
        [FrontAndCenterParam]
        public string HashFile { get; set; }

        [OperationParam]
        [CmdLineParameter(CmdlineParameterNames.InputDirectory)]
        [AtLeastOneOf("hashfile")]
        [OptionalEitherOr("input")]
        [ParameterDescription("Hashfile and Target files directory if Hashfile path is relative or not specified")]
        [FrontAndCenterParam]
        public override string InputDirectory { get; set; }

        [OperationParam]
        [CmdLineParameter(CmdlineParameterNames.InputFiles)]
        [OptionalEitherOr("input")]
        [ParameterDescription("Files to verify when hashfile comes with no file names")]
        [FrontAndCenterParam]
        public string[] InputFiles { get; set; }

        [OperationParam]
        [IniEntry(nameof(HashfileExtensions))]
        [Optional(defaultValue: ApplicationSettings.Defaults.HashfileExtension)]
        [ParameterDescription($"For hashfile lookup {nameof(InputDirectory)} when Hashfile is not explicitly specified")]
        [FrontAndCenterParam]
        public string[] HashfileExtensions { get; set; }

        [OperationParam]
        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional(defaultValue: HashFileReader.Default.HashfileEntrySeparator)]
        [ParameterDescription("A character sequence that separates File-name and Hash-value in a hashfile (given a hashfile contains file names). Put filename in quotes when it contains this separator character sequence")]
        [FrontAndCenterParam]
        public string HashfileEntrySeparator { get; set; }

        [CmdLineParameter("--ignore-extra")]
        [IniEntry(nameof(InputIgnoreExtraneous))]
        [Optional(defaultValue: false)]
        [ParameterDescription($"Whether to report any files provided with {nameof(InputDirectory)} or {nameof(InputFiles)} that are not in the hashfile")]
        public bool InputIgnoreExtraneous { get; set; }
    }
}