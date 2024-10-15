using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public static class SettingsProvider
    {
        /// <param name="profileName">If not specified, uses the profile specified in the settings file</param>
        public static IDictionary<string, string> ReadSettingsFile(FileInfo settingsFile, string profileName = null)
        {
            var iniReader = new Andy.Configuration.Ini.IniFileReader(
                new Andy.Configuration.Ini.IO.TextFileReader(),
                new Andy.Configuration.Ini.IniParser(
                    new Andy.Configuration.Ini.EntryParser()));

            var settingsDictionary = iniReader.Read(settingsFile);

            return GetSettingsDictionary(settingsDictionary, profileName);
        }

        /// <param name="profileName">If not specified, uses the profile specified in the settings file</param>
        public static IDictionary<string, string> GetSettingsDictionary(IDictionary<string, IDictionary<string, string>> settingsDictionary, string profileName = null)
        {
            if (!settingsDictionary.Any())
                return new Dictionary<string, string>();

            if (settingsDictionary.ContainsKey(""))
            {
                var root = settingsDictionary[""];

                profileName = (profileName
                        ?? (root.ContainsKey(ApplicationSettings.ProfileKey)
                            ? root[ApplicationSettings.ProfileKey]
                            : null))
                    ?.Trim();
                if (string.IsNullOrEmpty(profileName))
                    return root;

                if (!settingsDictionary.ContainsKey(profileName))
                    throw new ConfigurationException($"Configuration profile \"{profileName}\" was not found");

                var @overrides = settingsDictionary[profileName];

                // Override root value with those of the selected profile
                foreach (var (key, value) in overrides)
                    if (root.ContainsKey(key))
                        root[key] = value;
                    else
                        root.Add(key, value);

                return root;
            }
            else
            {
                if (string.IsNullOrEmpty(profileName) || !settingsDictionary.ContainsKey(profileName))
                    throw new ConfigurationException($"Configuration profile \"{profileName}\" was not found");

                return settingsDictionary[profileName];
            }
        }
    }
}