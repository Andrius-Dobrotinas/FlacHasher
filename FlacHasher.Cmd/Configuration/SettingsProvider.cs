using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public static class SettingsProvider
    {
        private static Settings GetSettings(
            IDictionary<string, IDictionary<string, string>> settings,
            string sectionName)
        {
            if (settings.ContainsKey(sectionName))
            {
                var section = settings[sectionName];

                return new Settings
                {
                    Decoder = GetFile(section, nameof(Settings.Decoder)),
                    DecoderParameters = GetValue(section, nameof(Settings.DecoderParameters)),
                    OutputFormat = GetValue(section, nameof(Settings.OutputFormat)),
                    ProcessExitTimeoutMs = GetValueInt(section, nameof(Settings.ProcessExitTimeoutMs)),
                    ProcessTimeoutSec = GetValueInt(section, nameof(Settings.ProcessTimeoutSec)),
                    ProcessStartWaitMs = GetValueInt(section, nameof(Settings.ProcessStartWaitMs)),
                    FailOnError = GetValueBool(section, nameof(Settings.FailOnError)),
                    HashfileExtensions = GetValue(section, nameof(Settings.HashfileExtensions)),
                    HashfileEntrySeparator = GetValue(section, nameof(Settings.HashfileEntrySeparator)),
                    HashAlgorithm = GetValue(section, nameof(Settings.HashAlgorithm)),
                    FileLookupIncludeHidden = GetValueBool(section, nameof(Settings.FileLookupIncludeHidden)) ?? false
                };
            }

            return new Settings();
        }

        private static FileInfo GetFile(IDictionary<string, string> section, string key)
        {
            var path = GetValue(section, key);

            return string.IsNullOrEmpty(path) ? null : new FileInfo(path);
        }

        private static string GetValue(IDictionary<string, string> section, string key)
        {
            return section.ContainsKey(key) 
                ? section[key]
                : null;
        }

        private static int? GetValueInt(IDictionary<string, string> section, string key)
        {
            var value = GetValue(section, key);

            if (string.IsNullOrWhiteSpace(value)) return null;

            return int.Parse(value);
        }

        private static bool? GetValueBool(IDictionary<string, string> section, string key)
        {
            var value = GetValue(section, key);

            if (string.IsNullOrWhiteSpace(value)) return null;

            return bool.Parse(value);
        }

        public static Settings GetSettings(FileInfo settingsFile)
        {
            var iniReader = new Configuration.Ini.IniFileReader(
                new Configuration.Ini.IO.TextFileReader(),
                new Configuration.Ini.IniParser(
                    new Configuration.Ini.EntryParser()));

            var settingsDictionary = iniReader.Read(settingsFile);

            return GetSettings(settingsDictionary, Configuration.Ini.IniParser.RootSectionName);
        }
    }
}