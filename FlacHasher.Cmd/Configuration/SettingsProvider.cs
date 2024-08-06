using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public static class SettingsProvider
    {
        private static Settings GetSettings(
            IDictionary<string, IDictionary<string, string>> settings,
            string sectionName)
        {
            if (!settings.ContainsKey(sectionName))
                return null;
            
            var section = settings[sectionName];

            return new Settings
            {
                Profile = GetValue(section, nameof(Settings.Profile)),
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

        private static FileInfo GetFile(IDictionary<string, string> section, string key)
        {
            var path = GetValue(section, key);

            return string.IsNullOrWhiteSpace(path) ? null : new FileInfo(path);
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

        public static Settings GetSettings(FileInfo settingsFile, string profileName = null)
        {
            var iniReader = new Configuration.Ini.IniFileReader(
                new Configuration.Ini.IO.TextFileReader(),
                new Configuration.Ini.IniParser(
                    new Configuration.Ini.EntryParser()));

            var settingsDictionary = iniReader.Read(settingsFile);

            var root = GetSettings(settingsDictionary, Configuration.Ini.IniParser.RootSectionName);
            if (root == null)
                return new Settings();

            var profile = profileName ?? root.Profile;
            if (profile != null && profile != ".")
            {
                if (!settingsDictionary.ContainsKey(profile))
                    throw new ConfigurationException($"Configuration profile \"{profile}\" was not found");
                
                var @override = GetSettings(settingsDictionary, profile);
                Merge(root, @override);
            }

            return root;
        }

        /// <summary>
        /// Copies non-null values from <paramref name="override"/> onto <paramref name="one"/>
        /// </summary>
        public static void Merge(Settings one, Settings @override)
        {
            var properties = typeof(Settings)
                .GetProperties()
                .Where(x => x.MemberType == System.Reflection.MemberTypes.Property);

            foreach (var property in properties)
            {
                var value = property.GetValue(@override);
                if (value != null)
                    property.SetValue(one, value);
            }
        }
    }
}