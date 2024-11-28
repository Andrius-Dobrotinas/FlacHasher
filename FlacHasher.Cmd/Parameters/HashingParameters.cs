using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andy.Cmd.Parameter;

namespace Andy.FlacHash.Application.Cmd
{
    public class HashingParameters : MasterParameters
    {
        [ParameterDescription($"A format which a resulting hash is presented in. When a format is not specified, it outputs the actual hash bytes. Use the following placeholders: {OutputFormatting.Placeholders.Hash}, {OutputFormatting.Placeholders.FileName}, {OutputFormatting.Placeholders.FilePath}")]
        [CmdLineParameter(CmdlineParameterNames.OutputFormat, Order = 0)]
        [IniEntry(nameof(OutputFormat), Order = 1)]
        [Optional]
        [FrontAndCenterParam]
        public string OutputFormat { get; set; }

        [ParameterDescription("A list of files to hash")]
        [CmdLineParameter(CmdlineParameterNames.InputFiles)]
        [EitherOr("input")]
        [FrontAndCenterParam]
        public string[] InputFiles { get; set; }

        [ParameterDescription($"A directory that contains files to hash. This has to be used in conjunction with {nameof(TargetFileExtension)}")]
        [CmdLineParameter(CmdlineParameterNames.InputDirectory)]
        [EitherOr("input")]
        [FrontAndCenterParam]
        public override string InputDirectory { get; set; }

        [ParameterDescription("Tells whether to continue processing files after failing to process one file")]
        [CmdLineParameter(CmdlineParameterNames.FailOnError, Order = 0)]
        [IniEntry(nameof(FailOnError), Order = 1)]
        [Optional]
        public bool FailOnError { get; set; }
    }
}