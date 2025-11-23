using Andy.Cmd.Parameter;
using Andy.FlacHash.Audio;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Cmd
{
    public abstract class MasterParameters : ApplicationSettings
    {
        [ConfigurationFacet(ApplicationSettings.ConfigurationFacet.Decoder)]
        [CmdLineParameter(CmdlineParameterNames.Decoder, Order = 0)]
        [IniEntry("Decoder", Order = 1)]
        [DecoderExeDescription]
        [FrontAndCenterParam]
        public string DecoderExe { get; set; }

        /// <summary>
        /// An array of parameters to <see cref="DecoderExe"/> exactly the way they are supposed to appear
        /// (with dashes and whatnot).
        /// There are default parameters defined in the code
        /// </summary>
        [ConfigurationFacet(ApplicationSettings.ConfigurationFacet.Decoder)]
        [CmdLineParameter(CmdlineParameterNames.DecoderParams, Order = 0)]
        [IniEntry(nameof(DecoderParameters), Order = 1)]
        [Optional]
        [DecoderParamsDescription]
        [FrontAndCenterParam]
        public string[] DecoderParameters { get; set; }

        public abstract string InputDirectory { get; set; }

        [OperationInstanceConfiguration]
        [CmdLineParameter(CmdlineParameterNames.FileExtensions, Order = 0)]
        [IniEntry(nameof(TargetFileExtensions), Order = 1)]
        [RequiredWith(nameof(InputDirectory))]
        [DecoderTargetFileExtensions]
        [FrontAndCenterParam]
        public string[] TargetFileExtensions { get; set; }

        [ConfigurationFacet(ApplicationSettings.ConfigurationFacet.Decoder)]
        [CmdLineParameter(CmdlineParameterNames.DecoderPrintProgress, Order = 0)]
        [IniEntry(nameof(PrintDecoderProgress), Order = 1)]
        [Optional(defaultValue: true)]
        [ParameterDescription("Tells whether the whole Audio Decoder informational output has to be redirected to the window or kept hidden for less noise")]
        public bool PrintDecoderProgress { get; set; }
    }
}