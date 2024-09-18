using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Andy.FlacHash.Cmd
{
    public static class SettingsProvider
    {
        private static Settings ReadSettingsSection(
            IDictionary<string, IDictionary<string, string>> settings,
            string sectionName)
        {
            return settings.ContainsKey(sectionName)
                ? ReadSettings(settings[sectionName])
                : null;
        }
            
        private static Settings ReadSettings(IDictionary<string, string> settings)
        {
            var result = new Settings();
            var properties = typeof(Settings).GetProperties();

            foreach (var property in properties)
            {
                if (settings.ContainsKey(property.Name))
                {
                    var valueParsed = Parse(settings[property.Name], property.PropertyType);
                    if (valueParsed != null)
                        property.SetValue(result, valueParsed);
                }
            }

            return result;
        }

        public static Settings GetSettings(FileInfo settingsFile, string profileName = null)
        {
            var iniReader = new Andy.Configuration.Ini.IniFileReader(
                new Andy.Configuration.Ini.IO.TextFileReader(),
                new Andy.Configuration.Ini.IniParser(
                    new Andy.Configuration.Ini.EntryParser()));

            var settingsDictionary = iniReader.Read(settingsFile);

            var root = ReadSettingsSection(settingsDictionary, Andy.Configuration.Ini.IniParser.RootSectionName);
            if (root == null)
                root = new Settings();

            var profile = profileName ?? root.Profile;
            if (profile != null && profile != ConfigurationProfile.RootProfileAlias)
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
            if (string.IsNullOrEmpty(profileName) || profileName == ConfigurationProfile.RootProfileAlias)
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

        public static ApplicationSettings Create(Parameters @params, Settings settings)
        {
            var paramsProperties = typeof(Parameters).GetProperties();
            var settingsFileProperties = typeof(Settings).GetProperties();
            var targetTypeProperties = typeof(ApplicationSettings).GetProperties();

            var result = new ApplicationSettings();
            foreach (var property in targetTypeProperties)
            {
                var paramProperty = paramsProperties.FirstOrDefault(x => x.Name == property.Name);
                var settingsFileProperty = settingsFileProperties.FirstOrDefault(x => x.Name == property.Name);

                if (paramProperty == null && settingsFileProperty == null)
                    throw new InvalidOperationException($"{nameof(ApplicationSettings)} field {property.Name} is not mapped to any params or {nameof(Settings)} field");

                var pValue = paramProperty?.GetValue(@params);
                var sValue = settingsFileProperty?.GetValue(settings);
                if (pValue != null) // if it has a value, means a valid value is provided via params and a settings file is redundant
                    SetValue(property, result, pValue);
                else if (sValue != null)
                    SetValue(property, result, sValue);
                else
                {
                    var optionalAttr = property.GetCustomAttribute<OptionalAttribute>();
                    if (optionalAttr != null)
                    {
                        if (optionalAttr.DefaultValue != null)
                            SetValue(property, result, optionalAttr.DefaultValue);
                        continue;
                    }
                }
            }

            // Required-when
            ParameterReader.CheckConditionallyRequiredOnes(result, targetTypeProperties);

            // Either-or
            var eitherOrPropertyGroups = ParameterReader.GetEitherOrPropertyGroups<ApplicationSettings>();
            ParameterReader.CheckEitherOrParameters(result, eitherOrPropertyGroups);

            // TODO: check optionality. whether all fields that are not marked with Optional have a non-null value!

            return result;
        }

        static void SetValue(PropertyInfo property, ApplicationSettings instance, object value)
        {
            try
            {
                property.SetValue(instance, value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error setting value (type: {value?.GetType()} to property {property.Name}: {ex.Message}");
            }
        }
    }
}