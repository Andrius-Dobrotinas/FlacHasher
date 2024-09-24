using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public static class SettingsProvider
    {
        public static IDictionary<string, string> GetSettingsDictionary(FileInfo settingsFile, string profileName = null)
        {
            var iniReader = new Andy.Configuration.Ini.IniFileReader(
                new Andy.Configuration.Ini.IO.TextFileReader(),
                new Andy.Configuration.Ini.IniParser(
                    new Andy.Configuration.Ini.EntryParser()));

            var settingsDictionary = iniReader.Read(settingsFile);

            if (!settingsDictionary.Any())
                return new Dictionary<string, string>();

            var root = settingsDictionary.First().Value;

            profileName = (profileName
                    ?? (root.ContainsKey(nameof(ApplicationSettings.Profile))
                        ? root[nameof(ApplicationSettings.Profile)]
                        : null))
                ?.Trim();
            if (string.IsNullOrEmpty(profileName) || profileName == ApplicationSettings.RootProfileAlias)
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
    }
}