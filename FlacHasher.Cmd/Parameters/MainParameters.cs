﻿using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Cmd
{
    public class MainParameters : ApplicationSettings
    {
        [CmdLineParameter(CmdlineParameterNames.Decoder, Order = 0)]
        [IniEntry("Decoder", Order = 1)]
        public string DecoderExe { get; set; }

        /// <summary>
        /// An array of parameters to <see cref="DecoderExe"/> exactly the way they are supposed to appear
        /// (with dashes and whatnot).
        /// There are default parameters defined in the code
        /// </summary>
        [IniEntry(nameof(DecoderParameters))]
        [Optional]
        public string[] DecoderParameters { get; set; }

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