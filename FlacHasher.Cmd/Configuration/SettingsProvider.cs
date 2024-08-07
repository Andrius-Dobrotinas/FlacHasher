using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public static class SettingsProvider
    {
        private static Settings ReadSettingsSection(
            IDictionary<string, IDictionary<string, string>> settings,
            string sectionName)
        {
            if (!settings.ContainsKey(sectionName))
                return null;
            
            var result = new Settings();
            var section = settings[sectionName];
            var properties = typeof(Settings).GetProperties();

            foreach (var entry in section)
            {
                var property = properties.FirstOrDefault(x => x.Name == entry.Key);
                if (property != null)
                {
                    var valueParsed = Parse(entry.Value, property.PropertyType);
                    if (valueParsed != null)
                        property.SetValue(result, valueParsed);
                }
            }

            return result;
        }

        public static Settings GetSettings(FileInfo settingsFile, string profileName = null)
        {
            var iniReader = new Configuration.Ini.IniFileReader(
                new Configuration.Ini.IO.TextFileReader(),
                new Configuration.Ini.IniParser(
                    new Configuration.Ini.EntryParser()));

            var settingsDictionary = iniReader.Read(settingsFile);

            var root = ReadSettingsSection(settingsDictionary, Configuration.Ini.IniParser.RootSectionName);
            if (root == null)
                return new Settings();

            var profile = profileName ?? root.Profile;
            if (profile != null && profile != ".")
            {
                if (!settingsDictionary.ContainsKey(profile))
                    throw new ConfigurationException($"Configuration profile \"{profile}\" was not found");
                
                var @override = ReadSettingsSection(settingsDictionary, profile);
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

        private static object Parse(string value, Type type)
        {
            if (value == null)
            {
                return null;
            }
            else if (type == typeof(string))
            {
                return value;
            }
            else if (type.IsGenericType && type.IsValueType)
            {
                var actualType = type.GenericTypeArguments.SingleOrDefault() ?? throw new NotSupportedException($"Expected a nullable value type to only have one generic type parameter: {type.FullName}");
                return Convert.ChangeType(value, actualType);
            }
            else if (type == typeof(FileInfo))
            {
                return new FileInfo(value);
            }

            throw new NotImplementedException($"Type {type.FullName}");
        }
    }
}