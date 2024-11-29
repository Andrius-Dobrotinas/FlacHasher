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
        [ParameterDescription($"Path to the hashfile. If it's: " +
            $"a) a full path - target files get taken from the same directory as the hashfile, or from {nameof(InputDirectory)} (if specified); " +
            $"b) relative path (just the file name) - {nameof(InputDirectory)} has to be specified (if not, hashfile and target files get looked up in \"current\" directory)")]
        [FrontAndCenterParam]
        public string HashFile { get; set; }

        [OperationParam]
        [CmdLineParameter(CmdlineParameterNames.InputDirectory)]
        [AtLeastOneOf("hashfile")]
        [OptionalEitherOr("input")]
        [ParameterDescription($"If hashfile is not explcitily specified, it would be looked up in this directory using {nameof(HashfileExtensions)}, along with target files. If hashfile full path is specified, only target files would be taken from this directory. Actual input files get looked up based on {nameof(TargetFileExtension)}")]
        [FrontAndCenterParam]
        public override string InputDirectory { get; set; }

        [OperationParam]
        [CmdLineParameter(CmdlineParameterNames.InputFiles)]
        [OptionalEitherOr("input")]
        [ParameterDescription("Files to verify when hashfile comes with no file names, for more specificity than throwing a whole directory at it")]
        [FrontAndCenterParam]
        public string[] InputFiles { get; set; }

        [OperationParam]
        [IniEntry(nameof(HashfileExtensions))]
        [Optional(defaultValue: ApplicationSettings.Defaults.HashfileExtension)]
        [ParameterDescription($"For hashfile lookup when it's not explicitly specified (ie when specifying just the {nameof(InputDirectory)}")]
        [FrontAndCenterParam]
        public string[] HashfileExtensions { get; set; }

        [OperationParam]
        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional(defaultValue: HashFileReader.Default.HashfileEntrySeparator)]
        [ParameterDescription($"A character sequence that separates File-name and Hash-value in a hashfile (given a hashfile contains file names)")]
        [FrontAndCenterParam]
        public string HashfileEntrySeparator { get; set; }

        [CmdLineParameter("--ignore-extra")]
        [IniEntry(nameof(InputIgnoreExtraneous))]
        [Optional(defaultValue: false)]
        [ParameterDescription($"Whether to report any files provided with {nameof(InputDirectory)} or {nameof(InputFiles)} that are not in the hashfile")]
        public bool InputIgnoreExtraneous { get; set; }
    }
}