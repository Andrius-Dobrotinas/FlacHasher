using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Cmd
{
    public class CmdApplicationParameters : ApplicationSettings
    {
        [CmdLineParameter(CmdlineParameterNames.OutputFormat, Order = 0)]
        [IniEntry(nameof(OutputFormat), Order = 1)]
        [Optional]
        public string OutputFormat { get; set; }

        [CmdLineParameter(CmdlineParameterNames.InputFiles)]
        [EitherOr("input")]
        public string[] InputFiles { get; set; }

        [CmdLineParameter(CmdlineParameterNames.InputDirectory)]
        [EitherOr("input")]
        public string InputDirectory { get; set; }

        [CmdLineParameter(CmdlineParameterNames.FileExtension, Order = 0)]
        [IniEntry(nameof(TargetFileExtension), Order = 1)]
        [RequiredWith(nameof(InputDirectory))]
        public string TargetFileExtension { get; set; }

        [CmdLineParameter(CmdlineParameterNames.FailOnError, Order = 0)]
        [IniEntry(nameof(FailOnError), Order = 1)]
        [Optional]
        public bool FailOnError { get; set; }

        [CmdLineParameter(CmdlineParameterNames.DecoderPrintProgress, Order = 0)]
        [IniEntry(nameof(PrintDecoderProgress), Order = 1)]
        [Optional(defaultValue: true)]
        public bool PrintDecoderProgress { get; set; }
    }
}