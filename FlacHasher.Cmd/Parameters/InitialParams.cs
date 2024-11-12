using Andy.Cmd.Parameter;
using Andy.FlacHash.Crypto;
using System;
using System.Collections.Generic;
using static Andy.FlacHash.Application.ApplicationSettings;

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
        [AllowEmpty] // To override what's in the settings file
        [EitherOr("decoder", AllowNone = true)]
        public string DecoderProfile { get; set; }

        /// <summary>
        /// Purely as a placeholder for the one in <see cref="MainParameters"/>
        /// for parameter validations: to prevent specifying both parameters
        /// </summary>
        [CmdLineParameter(CmdlineParameterNames.Decoder)]
        [EitherOr("decoder", AllowNone = true)]
        public string DecoderExe { get; set; }

        [CmdLineParameter(CmdlineParameterNames.HashingProfile)]
        [AllowEmpty]
        [EitherOr("hashAlgorithm", AllowNone = true)]
        public string HashingProfile { get; set; }

        [CmdLineParameter(CmdlineParameterNames.HashAlgorithm)]
        [EitherOr("hashAlgorithm", AllowNone = true)]
        public Algorithm? HashAlgorithm { get; set; }

        [CmdLineParameter(CmdlineParameterNames.ModeVerify)]
        [Optional]
        public bool IsVerification { get; set; }
    }
}