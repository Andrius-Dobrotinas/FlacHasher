using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Cmd
{
    public abstract class MasterParameters : ApplicationSettings
    {
        [CmdLineParameter(CmdlineParameterNames.Decoder, Order = 0)]
        [IniEntry("Decoder", Order = 1)]
        [ParameterDescription("Path to the Audio decoder executable file; if just a file name is given (without a path), it gets looked up in paths configured in PATH environment variable. If not specified, the value is assumed to be \"flac.exe\"")]
        [FrontAndCenterParam]
        public string DecoderExe { get; set; }

        /// <summary>
        /// An array of parameters to <see cref="DecoderExe"/> exactly the way they are supposed to appear
        /// (with dashes and whatnot).
        /// There are default parameters defined in the code
        /// </summary>
        [IniEntry(nameof(DecoderParameters))]
        [Optional]
        [ParameterDescription("An array of parameters to the Audio decoder exactly the way they are supposed to appear (with dashes and whatnot), separated by a semi-colon. If not specified, default FLAC parameters are used, but this HAS to be specified for other decoders")]
        [FrontAndCenterParam]
        public string[] DecoderParameters { get; set; }

        public abstract string InputDirectory { get; set; }

        [CmdLineParameter(CmdlineParameterNames.FileExtension, Order = 0)]
        [IniEntry(nameof(TargetFileExtension), Order = 1)]
        [RequiredWith(nameof(InputDirectory))]
        [ParameterDescription("Extension of a file type that is accepted by the configured Audio decoder")]
        [FrontAndCenterParam]
        public string TargetFileExtension { get; set; }

        [CmdLineParameter(CmdlineParameterNames.DecoderPrintProgress, Order = 0)]
        [IniEntry(nameof(PrintDecoderProgress), Order = 1)]
        [Optional(defaultValue: true)]
        [ParameterDescription("Tells whether the whole Audio Decoder informational output has to be redirected to the window or kept hidden for less noise")]
        public bool PrintDecoderProgress { get; set; }
    }
}