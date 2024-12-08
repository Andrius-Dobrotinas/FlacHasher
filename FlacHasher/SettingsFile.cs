using System;
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
        public static IDictionary<string, string> GetSettingsProfile(IDictionary<string, IDictionary<string, string>> settingsDictionary, string profileName = null, bool caseInsensitive = false)
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

                var @overrides = TryGetValue(settingsDictionary, profileName, out bool sectionFound, caseInsensitive);
                if (!sectionFound)
                    throw new ConfigurationException($"Configuration profile \"{profileName}\" was not found");

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

            var settings = GetSettingsProfile(settingsDictionary, profileName, caseInsensitive: true);

            var decoderSectionName = ResolveConfigValue(settings, ApplicationSettings.DecoderProfileKey, decoderProfileName, ApplicationSettings.DefaultDecoderSection, caseInsensitive: true);
            MergeSectionValuesIn(settings, settingsDictionary, BuildSectionName(ApplicationSettings.DecoderSectionPrefix, decoderSectionName), caseInsensitive: true, isMandatory: false);

            var hashingSectionName = ResolveConfigValue(settings, ApplicationSettings.HashingProfileKey, hashingProfileName, ApplicationSettings.DefaultHashingSection, caseInsensitive: true);
            MergeSectionValuesIn(settings, settingsDictionary, BuildSectionName(ApplicationSettings.HashingSectionPrefix, hashingSectionName), caseInsensitive: true, isMandatory: false);

            return settings;
        }

        public static string BuildSectionName(string prefix, string name)
            => string.IsNullOrWhiteSpace(name) ? prefix : $"{prefix}.{name}";

        public static string ResolveConfigValue(IDictionary<string, string> settings, string configKey, string preferredValue, string defaultValue, bool caseInsensitive = false)
        {
            if (preferredValue == "")
                return defaultValue;
            else if (preferredValue != null)
                return preferredValue;
            else
                return TryGetValue(settings, configKey, out bool _, caseInsensitive)
                    ?? defaultValue;
        }

        public static void MergeSectionValuesIn(IDictionary<string, string> destination, IDictionary<string, IDictionary<string, string>> wholeSettingsFileDictionary, string targetSectionName, bool caseInsensitive = false, bool isMandatory = true)
        {
            var targetSection = TryGetValue(wholeSettingsFileDictionary, targetSectionName, out bool sectionFound, caseInsensitive);
            if (!sectionFound)
                if (isMandatory)
                    throw new ConfigurationException($"Configuration section not found: {targetSectionName}");
                else
                    return;
            Merge(destination, targetSection);
        }

        static void Merge(IDictionary<string, string> target, IDictionary<string, string> overrides)
        {
            foreach (var (key, value) in overrides)
                if (target.ContainsKey(key))
                    target[key] = value;
                else
                    target.Add(key, value);
        }

        static TValue TryGetValueCaseInsensitively<TValue>(IDictionary<string, TValue> dictionary, string sectionName, out bool containsSection)
        {
            var keys = dictionary.Keys.ToDictionary(x => x.ToLowerInvariant(), x => x);
            var keyLowercase = sectionName.ToLowerInvariant();

            containsSection = keys.ContainsKey(keyLowercase);
            if (!containsSection)
                return default;

            return dictionary[keys[keyLowercase]];
        }

        static TValue TryGetValue<TValue>(IDictionary<string, TValue> dictionary, string sectionName, out bool containsSection)
        {
            containsSection = dictionary.ContainsKey(sectionName);
            if (!containsSection)
                return default;

            return dictionary[sectionName];
        }

        static TValue TryGetValue<TValue>(IDictionary<string, TValue> dictionary, string sectionName, out bool containsSection, bool caseInsensitive)
        {
            return caseInsensitive
                ? TryGetValueCaseInsensitively(dictionary, sectionName, out containsSection)
                : TryGetValue(dictionary, sectionName, out containsSection);
        }
    }
}