using Andy.Cmd.Parameter;
using Andy.FlacHash.Crypto;
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
        [ParameterDescription("Settings profile to use. Overrides the value provided in the settings file. Empty value results in overriding a pre-configured Profile value to use the default one")]
        public string Profile { get; set; }

        [CmdLineParameter(CmdlineParameterNames.DecoderProfile)]
        [AllowEmpty] // To override what's in the settings file
        [OptionalEitherOr("decoder")]
        [ParameterDescription("Name of a pre-configured decoder profile. Overrides the value provided in the settings file. Empty value results in overriding a pre-configured Profile value to use the default one")]
        public string DecoderProfile { get; set; }

        /// <summary>
        /// Purely as a placeholder for the one in <see cref="MasterParameters"/>
        /// for parameter validations: to prevent specifying both parameters
        /// </summary>
        [CmdLineParameter(CmdlineParameterNames.Decoder)]
        [OptionalEitherOr("decoder")]
        public string DecoderExe { get; set; }

        [CmdLineParameter(CmdlineParameterNames.HashingProfile)]
        [AllowEmpty]
        [OptionalEitherOr("hashAlgorithm")]
        [ParameterDescription("Name of a pre-configured hashing profile. Overrides the value provided in the settings file. Empty value results in overriding a pre-configured Profile value to use the default one")]
        public string HashingProfile { get; set; }

        [CmdLineParameter(CmdlineParameterNames.HashAlgorithm)]
        [OptionalEitherOr("hashAlgorithm")]
        public Algorithm? HashAlgorithm { get; set; }

        [CmdLineParameter(CmdlineParameterNames.ModeVerify)]
        [Optional]
        public bool IsVerification { get; set; }

        [CmdLineParameter(CmdlineParameterNames.ModeHelp)]
        [Optional]
        public bool IsHelp { get; set; }
    }
}