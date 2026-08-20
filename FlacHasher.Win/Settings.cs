using Andy.Cmd.Parameter;
using Andy.FlacHash.Hashfile.Read;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Win
{
    public class Settings : ApplicationSettings
    {
        [IniEntry(nameof(HashfileExtensions))]
        [Optional(defaultValue: ApplicationSettings.Defaults.HashfileExtension)]
        [ParameterDescription($"For hashfile lookup when it's not explicitly specified (ie when opening a direcotry, not specific files)")]
        [ConfigurationFacet(ConfigurationFacet.Verification)]
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(ShowProcessWindowWithOutput))]
        [Optional]
        [ConfigurationFacet(ConfigurationFacet.Decoder)]
        public bool ShowProcessWindowWithOutput { get; set; }

        [IniEntry(nameof(FailOnError))]
        [Optional]
        [ParameterDescription("Whether to keep going when there's a problem processing one file in a batch")]
        [ConfigurationFacet(ConfigurationFacet.Decoder)]
        public bool FailOnError { get; set; }

        [IniEntry(nameof(OutputFormat))]
        [Optional(defaultValue: "{name} {hash}")]
        [ParameterDescription($"A format which hash result is saved in. Use the following placeholders: {OutputFormatting.Placeholders.Hash}, {OutputFormatting.Placeholders.FileName}, {OutputFormatting.Placeholders.FilePath}")]
        [ConfigurationFacet(ConfigurationFacet.Hashing)]
        public string OutputFormat { get; set; }
    }
}