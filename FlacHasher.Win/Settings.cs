using Andy.Cmd.Parameter;
using Andy.FlacHash.Cmd;
using System;
using System.Collections.Generic;
using System.Text;

namespace Andy.FlacHash.Win
{
    public class Settings : ApplicationSettings
    {
        [IniEntry(nameof(HashfileExtensions))]
        [Optional]
        public string[] HashfileExtensions { get; set; }

        [IniEntry(nameof(HashfileEntrySeparator))]
        [Optional]
        public string HashfileEntrySeparator { get; set; }
    }
}
