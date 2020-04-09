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
            if (settings.ContainsKey(defaultSectionName))
            {
                var defaultSection = settings[defaultSectionName];

                if (defaultSection.ContainsKey(nameof(Settings.Decoder)))
                {
                    var decoderPath = defaultSection[nameof(Settings.Decoder)];

                    if (!string.IsNullOrEmpty(decoderPath))
                        decoder = new FileInfo(decoderPath);
                }
            }

            return new Settings
            {
                Decoder = decoder
            };
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