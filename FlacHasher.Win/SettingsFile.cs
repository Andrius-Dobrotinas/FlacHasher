using Andy.Cmd.Parameter;
using Andy.FlacHash.Application.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Application.Win
{
    internal class SettingsFile
    {
        public static (Settings, DecoderProfile[]) GetSettings(FileInfo settingsFile)
        {
            var settings = GetApplicationSettings();

            var decoderProfilesIni = ReadIniFile(settingsFile);
            var decoderProfiles = ParseDecoderProfiles(decoderProfilesIni).ToArray();

            return (settings, decoderProfiles);
        }

        private static Settings GetApplicationSettings()
        {
            var settings = Properties.Default.ApplicationSettings;

            if (settings != null)
                return settings;
            else
            {
                using (var settingsForm = new SettingsForm(new Settings()))
                {
                    var result = settingsForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        settings = settingsForm.Result;
                        Properties.Default.ApplicationSettings = settings;
                        Properties.Default.Save();
                        return settings;
                    }
                    else
                    {
                        throw new OperationCanceledException();
                    }
                }
            }
        }

        public static IDictionary<string, Dictionary<string, string[]>> ReadIniFile(FileInfo settingsFile)
        {
            var wholeSettingsFileDictionary = Configuration.Ini.IniFileReader.Default.ReadIniFile(settingsFile);
            if (!wholeSettingsFileDictionary.Any())
                return null;

            var decoderProfiles = wholeSettingsFileDictionary.Where(x => x.Key.StartsWith(ApplicationSettings.DecoderSectionPrefix, StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.ToDictionary(
                        i => i.Key,
                        i => new[] { i.Value }));

            return decoderProfiles;
        }

        public static IList<DecoderProfile> ParseDecoderProfiles(IDictionary<string, Dictionary<string, string[]>> decoderProfilesRaw)
        {
            var paramReader = ParameterReader.Build();

            var profilesFromIni = (decoderProfilesRaw != null && decoderProfilesRaw.Any()) ? Get(decoderProfilesRaw, paramReader) : Array.Empty<DecoderProfile>();

            var profilesFromUserProfile = Properties.Default.DecoderProfiles?.Profiles != null ? Properties.Default.DecoderProfiles.Profiles : Array.Empty<DecoderProfile>();

            if (!profilesFromIni.Any() && !profilesFromUserProfile.Any())
                return PromptUserToCreateProfile();

            return MergeProfiles(profilesFromIni, profilesFromUserProfile);
        }

        /// <summary>
        /// Merge, preferring profiles from INI
        /// </summary>
        static IList<DecoderProfile> MergeProfiles(IList<DecoderProfile> profilesFromIni, IList<DecoderProfile> profilesFromUserProfile)
        {
            var merged = new List<DecoderProfile>(profilesFromIni);
            var iniNames = new HashSet<string>(profilesFromIni.Select(p => p.Name), StringComparer.InvariantCultureIgnoreCase);
            foreach (var profile in profilesFromUserProfile)
            {
                if (!iniNames.Contains(profile.Name))
                    merged.Add(profile);
            }

            return merged;
        }

        static IList<DecoderProfile> Get(IDictionary<string, Dictionary<string, string[]>> decoderProfilesRaw, ParameterReader paramReader)
        {
            return decoderProfilesRaw
                    .Select(profileSection =>
                    {
                        var profileRaw = paramReader.GetParameters<DecoderProfileIniSection>(profileSection.Value);

                        return new DecoderProfile
                        {
                            Name = profileSection.Key.Replace($"{ApplicationSettings.DecoderSectionPrefix}.", "", StringComparison.InvariantCultureIgnoreCase),
                            Decoder = profileRaw.DecoderExe,
                            DecoderParameters = profileRaw.DecoderParameters,
                            TargetFileExtensions = profileRaw.TargetFileExtensions
                        };
                    })
                    .ToArray();
        }

        static IList<DecoderProfile> PromptUserToCreateProfile()
        {
            using (var dialog = new UI.DecoderProfileDialog(new DecoderProfile
            {
                Name = "FLAC",
                Decoder = "flac.exe",
                DecoderParameters = Audio.Flac.Parameters.Decode.Stream,
                TargetFileExtensions = new string[] { "flac" }
            }))
            {
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    DecoderProfile[] profiles = { dialog.Profile };
                    Properties.Default.DecoderProfiles = new DecoderProfileList { Profiles = profiles };
                    Properties.Default.Save();
                    return profiles;
                }
                else
                    throw new OperationCanceledException();
            }
        }

        public class DecoderProfileIniSection
        {
            [IniEntry("Decoder")]
            [Cmd.Parameters.DecoderExeDescription]
            public virtual string DecoderExe { get; set; }

            [IniEntry(nameof(DecoderParameters))]
            [Cmd.Parameters.DecoderParamsDescription]
            public virtual string[] DecoderParameters { get; set; }

            [IniEntry(nameof(TargetFileExtensions))]
            [Cmd.Parameters.DecoderTargetFileExtensions]
            public virtual string[] TargetFileExtensions { get; set; }
        }
    }
}