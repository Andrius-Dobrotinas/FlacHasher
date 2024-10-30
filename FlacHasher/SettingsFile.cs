﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Application
{
    public static class SettingsFile
    {
        /// <summary>
        /// Reads a given INI <paramref name="settingsFile"/> and returns a profile specified by <paramref name="profileName"/>
        /// or, if empty, configured in the file.
        /// </summary>
        /// <param name="profileName">If not specified, uses the profile specified in the settings file</param>
        public static IDictionary<string, string> ReadIniFile(FileInfo settingsFile, string profileName = null, string decoderProfileName = null, string hashingProfileName = null)
        {
            var settingsDictionary = Configuration.Ini.IniFileReader.Default.ReadIniFile(settingsFile);

            return GetSettingsForCmdline(settingsDictionary, profileName, decoderProfileName, hashingProfileName);
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
                Merge(root, overrides);

                return root;
            }
            else
            {
                if (string.IsNullOrEmpty(profileName) || !settingsDictionary.ContainsKey(profileName))
                    throw new ConfigurationException($"Configuration profile \"{profileName}\" was not found");

                return settingsDictionary[profileName];
            }
        }

        public static IDictionary<string, string> GetSettingsForCmdline(IDictionary<string, IDictionary<string, string>> settingsDictionary, string profileName = null, string decoderProfileName = null, string hashingProfileName = null)
        {
            if (!settingsDictionary.Any())
                return new Dictionary<string, string>();

            var settings = GetSettingsProfile(settingsDictionary, profileName);

            var decoderSectionName = ResolveConfigValue(settings, ApplicationSettings.DecoderProfileKey, decoderProfileName, ApplicationSettings.DefaultDecoderSection);
            MergeSectionValuesIn(settings, settingsDictionary, decoderSectionName);

            var hashingSectionName = ResolveConfigValue(settings, ApplicationSettings.HashingProfileKey, hashingProfileName, ApplicationSettings.DefaultHashingSection);
            MergeSectionValuesIn(settings, settingsDictionary, hashingSectionName);

            return settings;
        }

        public static string ResolveConfigValue(IDictionary<string, string> settings, string configKey, string preferredValue, string defaultValue)
        {
            if (preferredValue == "")
                return defaultValue;
            else if (preferredValue != null)
                return preferredValue;
            else
                return settings.ContainsKey(configKey)
                    ? settings[configKey]
                    : defaultValue;
        }

        public static void MergeSectionValuesIn(IDictionary<string, string> destination, IDictionary<string, IDictionary<string, string>> wholeSettingsFileDictionary, string targetSectionName)
        {
            if (!wholeSettingsFileDictionary.ContainsKey(targetSectionName))
                throw new ConfigurationException($"Configuration section not found: {targetSectionName}");

            var decoderSection = wholeSettingsFileDictionary[targetSectionName];
            Merge(destination, decoderSection);
        }

        static void Merge(IDictionary<string, string> target, IDictionary<string, string> overrides)
        {
            foreach (var (key, value) in overrides)
                if (target.ContainsKey(key))
                    target[key] = value;
                else
                    target.Add(key, value);
        }
    }
}