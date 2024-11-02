using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Cmd
{
    public class InitialParams
    {
        /// <summary>
        /// When non-null, specifies a settings profile to use.
        /// Overrides one configured in the settings file.
        /// Empty value results in overriding a pre-configured Profile value to use the default one.
        /// </summary>
        [CmdLineParameter(CmdlineParameterNames.Profile)]
        [Optional]
        [AllowEmpty]
        public string Profile { get; set; }

        [CmdLineParameter(CmdlineParameterNames.DecoderProfile)]
        [AllowEmpty]
        [Optional]
        public string DecoderProfile { get; set; }

        [CmdLineParameter(CmdlineParameterNames.HashingProfile)]
        [AllowEmpty]
        [Optional]
        public string HashingProfile { get; set; }

        [CmdLineParameter(CmdlineParameterNames.ModeVerify)]
        [Optional]
        public bool IsVerification { get; set; }
    }
}