using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash
{
    public static class SettingsFile
    {
        /// <summary>
        /// Reads a given INI <paramref name="settingsFile"/> and returns a profile specified by <paramref name="profileName"/>
        /// or, if empty, configured in the file.
        /// </summary>
        /// <param name="profileName">If not specified, uses the profile specified in the settings file</param>
        public static IDictionary<string, string> ReadIniFile(FileInfo settingsFile, string profileName = null)
        {
            var settingsDictionary = Configuration.Ini.IniFileReader.Default.ReadIniFile(settingsFile);

            return GetSettingsProfile(settingsDictionary, profileName);
        }

        /// <summary>
        /// Reads a specified settings profile from <paramref name="settingsDictionary"/> merging it with the root settings node.
        /// If no <paramref name="profileName"/> is specified, uses a profile configured in the root entry of <paramref name="settingsDictionary"/>.
        /// If none is configured there, then simply returns the root settings node.
        /// </summary>
        public static IDictionary<string, string> GetSettingsProfile(IDictionary<string, IDictionary<string, string>> settingsDictionary, string profileName = null)
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