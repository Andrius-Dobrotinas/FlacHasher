using Andy.Cmd.Parameter;
using Andy.FlacHash.Hashing.Verification;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application.Win
{
    public class Settings : ApplicationSettings
    {
        [IniEntry(nameof(HashfileExtensions))]
        [Optional(defaultValue: FileHashMap.DefaultExtension)]
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional(defaultValue: HashFileReader.Default.HashfileEntrySeparator)]
        public string HashfileEntrySeparator { get; set; }

        [IniEntry(nameof(TargetFileExtension))]
        [Optional]
        public string TargetFileExtension { get; set; }

        [IniEntry(nameof(ShowProcessWindowWithOutput))]
        [Optional]
        public bool ShowProcessWindowWithOutput { get; set; }
    }
}
