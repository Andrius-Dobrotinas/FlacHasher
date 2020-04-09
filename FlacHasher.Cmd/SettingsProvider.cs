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
            FileInfo decoder = null;
            string outputFormat = null;

            if (settings.ContainsKey(defaultSectionName))
            {
                var defaultSection = settings[defaultSectionName];

                decoder = GetFile(defaultSection, nameof(Settings.Decoder));
                outputFormat = GetValue(defaultSection, nameof(Settings.OutputFormat));
            }

            return new Settings
            {
                Decoder = decoder,
                OutputFormat = outputFormat
            };
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

        public static Settings GetSettings(FileInfo settingsFile)
        {
            var iniReader = new Configuration.Ini.IniFileReader(
                new IO.TextFileReader(),
                new Configuration.Ini.IniParser(
                    new Configuration.Ini.EntryParser()));

            var settingsDictionary = iniReader.Read(settingsFile);

            return GetSettings(settingsDictionary, Configuration.Ini.IniParser.DefaultSectionName);
        }
    }
}