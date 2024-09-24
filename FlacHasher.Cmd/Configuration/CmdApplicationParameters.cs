using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Andy.FlacHash.Cmd
{
    public class CmdApplicationParameters : ApplicationSettings
    {
        [CmdLineParameter(ParameterNames.OutputFormat, Order = 0)]
        [IniEntry(nameof(OutputFormat), Order = 1)]
        [Optional]
        public string OutputFormat { get; set; }

        [CmdLineParameter(ParameterNames.InputFiles)]
        [EitherOr("input")]
        public string[] InputFiles { get; set; }

        [CmdLineParameter(ParameterNames.InputDirectory)]
        [EitherOr("input")]
        public string InputDirectory { get; set; }

        [CmdLineParameter(ParameterNames.FileExtension, Order = 0)]
        [IniEntry(nameof(TargetFileExtension), Order = 1)]
        [RequiredWith(nameof(InputDirectory))]
        public string TargetFileExtension { get; set; }
    }
}