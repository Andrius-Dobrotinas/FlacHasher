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
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional(defaultValue: HashFileReader.Default.HashfileEntrySeparator)]
        public string HashfileEntrySeparator { get; set; }

        [IniEntry(nameof(ShowProcessWindowWithOutput))]
        [Optional]
        public bool ShowProcessWindowWithOutput { get; set; }

        [IniEntry(nameof(FailOnError))]
        [Optional]
        public bool FailOnError { get; set; }
    }
}
