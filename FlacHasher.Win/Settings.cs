using Andy.Cmd.Parameter;
using Andy.FlacHash.Cmd;
using Andy.FlacHash.Hashing.Verification;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Win
{
    public class Settings : ApplicationSettings
    {
        [IniEntry(nameof(HashfileExtensions))]
        [Optional(defaultValue: FileHashMap.DefaultExtension)]
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional]
        public string HashfileEntrySeparator { get; set; }
    }
}
