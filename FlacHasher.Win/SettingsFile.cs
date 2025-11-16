using Andy.Cmd.Parameter;
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
            var decoderProfilesIni = ReadIniFile(settingsFile);

            var initialDecoderProfiles = ParseDecoderProfiles(decoderProfilesIni).ToArray();

            var settings = GetApplicationSettings(initialDecoderProfiles);

            var decoderProfiles = ParseDecoderProfiles(decoderProfilesIni).ToArray();

            return (settings, decoderProfiles);
        }

        private static Settings GetApplicationSettings(IList<DecoderProfile> decoderProfiles = null)
        {
            var settings = Properties.Default.ApplicationSettings;

            if (settings != null)
                return settings;
            else
            {
                using (var settingsForm = new UI.SettingsForm(ParamUtil.CreateWithDefaults<Settings>(), decoderProfiles))
                {
                    var result = settingsForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        settings = settingsForm.Result;
                        Properties.Default.ApplicationSettings = settings;

                        Properties.Default.DecoderProfiles = new DecoderProfileList
                        {
                            Profiles = settingsForm.ResultDecoderProfiles.ToArray()
                        };

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

        public static IList<DecoderProfile> ParseDecoderProfiles(IDictionary<string, Dictionary<string, string[]>> decoderProfilesFromIni)
        {
            var paramReader = ParameterReader.Build();

            var profilesFromIni = (decoderProfilesFromIni != null && decoderProfilesFromIni.Any()) ? Get(decoderProfilesFromIni, paramReader) : Array.Empty<DecoderProfileIniSection>();

            var profilesFromUserProfile = Properties.Default.DecoderProfiles?.Profiles != null ? Properties.Default.DecoderProfiles.Profiles : Array.Empty<DecoderProfile>();

            if (!profilesFromIni.Any() && !profilesFromUserProfile.Any())
                return PromptUserToCreateProfile();

            return MergeProfiles(profilesFromIni, profilesFromUserProfile);
        }

        /// <summary>
        /// Merge, preferring profiles from INI
        /// </summary>
        static IList<DecoderProfile> MergeProfiles(IList<DecoderProfileIniSection> profilesFromIni, IList<DecoderProfile> profilesFromUserProfile)
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

        static IList<DecoderProfileIniSection> Get(IDictionary<string, Dictionary<string, string[]>> decoderProfilesRaw, ParameterReader paramReader)
        {
            return decoderProfilesRaw
                    .Select(profileSection =>
                    {
                        var profileRaw = paramReader.GetParameters<DecoderProfileIniSection>(profileSection.Value);
                        profileRaw.Name = profileSection.Key.Replace($"{ApplicationSettings.DecoderSectionPrefix}.", "", StringComparison.InvariantCultureIgnoreCase);

                        return profileRaw;
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
    }
}