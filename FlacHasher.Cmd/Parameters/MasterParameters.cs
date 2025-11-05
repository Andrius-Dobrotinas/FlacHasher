using Andy.Cmd.Parameter;
using Andy.FlacHash.Audio;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Cmd
{
    public abstract class MasterParameters : ApplicationSettings
    {
        [DecoderParam]
        [CmdLineParameter(CmdlineParameterNames.Decoder, Order = 0)]
        [IniEntry("Decoder", Order = 1)]
        [ParameterDescription("Path to the Audio Decoder executable file. It can be either a) a Full path or b) name with no path for look-up in paths configured in PATH environment variable")]
        [FrontAndCenterParam]
        public string DecoderExe { get; set; }

        /// <summary>
        /// An array of parameters to <see cref="DecoderExe"/> exactly the way they are supposed to appear
        /// (with dashes and whatnot).
        /// There are default parameters defined in the code
        /// </summary>
        [DecoderParam]
        [CmdLineParameter(CmdlineParameterNames.DecoderParams, Order = 0)]
        [IniEntry(nameof(DecoderParameters), Order = 1)]
        [Optional]
        [ParameterDescription($"An array of parameters to the Audio decoder (to process a single file), exactly the way they are supposed to appear (with dashes and whatnot), but separated by semi-colons instead of spaces. Filename placeholder: \"{DecoderParameter.FilePlaceholder}\"; alternatively, data can be fed via stdin - use the approrpiate decoder parameter for that")]
        [FrontAndCenterParam]
        public string[] DecoderParameters { get; set; }

        public abstract string InputDirectory { get; set; }

        [OperationParam]
        [CmdLineParameter(CmdlineParameterNames.FileExtensions, Order = 0)]
        [IniEntry(nameof(TargetFileExtensions), Order = 1)]
        [RequiredWith(nameof(InputDirectory))]
        [ParameterDescription("File types that are accepted by the configured Audio decoder (semi-colon-separated). Only for file lookup when specifying an input Directory. This can be stored with each Audio Decoder profile")]
        [FrontAndCenterParam]
        public string[] TargetFileExtensions { get; set; }

        [DecoderParam]
        [CmdLineParameter(CmdlineParameterNames.DecoderPrintProgress, Order = 0)]
        [IniEntry(nameof(PrintDecoderProgress), Order = 1)]
        [Optional(defaultValue: true)]
        [ParameterDescription("Tells whether the whole Audio Decoder informational output has to be redirected to the window or kept hidden for less noise")]
        public bool PrintDecoderProgress { get; set; }
    }
}