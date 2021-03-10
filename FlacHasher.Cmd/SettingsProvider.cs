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

                var decoder = GetFile(defaultSection, nameof(Settings.Decoder));
                var outputFormat = GetValue(defaultSection, nameof(Settings.OutputFormat));
                var processTimeoutSec = GetValueInt(defaultSection, nameof(Settings.ProcessTimeoutSec));

                return new Settings
                {
                    Decoder = decoder,
                    OutputFormat = outputFormat,
                    ProcessTimeoutSec = processTimeoutSec
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