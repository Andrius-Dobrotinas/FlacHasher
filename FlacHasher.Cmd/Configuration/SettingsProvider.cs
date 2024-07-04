using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public static class SettingsProvider
    {
        private static Settings GetSettings(
            IDictionary<string, IDictionary<string, string>> settings,
            string defaultSectionName)
        {
            if (settings.ContainsKey(defaultSectionName))
            {
                var defaultSection = settings[defaultSectionName];

                return new Settings
                {
                    Decoder = GetFile(defaultSection, nameof(Settings.Decoder)),
                    OutputFormat = GetValue(defaultSection, nameof(Settings.OutputFormat)),
                    ProcessExitTimeoutMs = GetValueInt(defaultSection, nameof(Settings.ProcessExitTimeoutMs)),
                    ProcessTimeoutSec = GetValueInt(defaultSection, nameof(Settings.ProcessTimeoutSec)),
                    ProcessStartWaitMs = GetValueInt(defaultSection, nameof(Settings.ProcessStartWaitMs)),
                    FailOnError = GetValueBool(defaultSection, nameof(Settings.FailOnError)),
                    HashfileExtensions = GetValue(defaultSection, nameof(Settings.HashfileExtensions))
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

            return GetSettings(settingsDictionary, Configuration.Ini.IniParser.DefaultSectionName);
        }
    }
}