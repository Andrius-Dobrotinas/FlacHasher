using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andy.Cmd.Parameter;

namespace Andy.FlacHash.Application.Cmd
{
    public class HashingParameters : MainParameters
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
        public override string InputDirectory { get; set; }
    }
}