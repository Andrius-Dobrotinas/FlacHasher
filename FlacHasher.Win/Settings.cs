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
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional(defaultValue: HashFileReader.Default.HashfileEntrySeparator)]
        [ParameterDescription("A character sequence that separates File-name and Hash-value in a hashfile (given a hashfile contains file names)")]
        public string HashfileEntrySeparator { get; set; }

        [IniEntry(nameof(ShowProcessWindowWithOutput))]
        [Optional]
        public bool ShowProcessWindowWithOutput { get; set; }

        [IniEntry(nameof(FailOnError))]
        [Optional]
        public bool FailOnError { get; set; }

        [IniEntry(nameof(OutputFormat))]
        [Optional(defaultValue: "{name}:{hash}")]
        [ParameterDescription($"A format which hash result is saved in. Use the following placeholders: {OutputFormatting.Placeholders.Hash}, {OutputFormatting.Placeholders.FileName}, {OutputFormatting.Placeholders.FilePath}")]
        public string OutputFormat { get; set; }
    }
}
