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
            var (settingsRaw, decoderProfilesRaw) = ReadIniFile(settingsFile);
            return ParseSettings(settingsRaw, decoderProfilesRaw);
        }

        public static (IDictionary<string, string[]>, IDictionary<string, Dictionary<string, string[]>>) ReadIniFile(FileInfo settingsFile, string profileName = null)
        {
            var wholeSettingsFileDictionary = Configuration.Ini.IniFileReader.Default.ReadIniFile(settingsFile);
            if (!wholeSettingsFileDictionary.Any())
                throw new ConfigurationException("The Configuration file is empty");

            var settings = Application.SettingsFile.GetSettingsProfile(wholeSettingsFileDictionary, profileName, caseInsensitive: true);
            Application.SettingsFile.MergeSectionValuesIn(settings, wholeSettingsFileDictionary, Application.SettingsFile.BuildSectionName(ApplicationSettings.HashingSectionPrefix, ApplicationSettings.DefaultHashingSection), isMandatory: false);

            var decoderProfiles = wholeSettingsFileDictionary.Where(x => x.Key.StartsWith("Decoder", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(
                    x => x.Key, 
                    x => x.Value.ToDictionary (
                        i => i.Key,
                        i => new[] { i.Value }));

            return (settings.ToDictionary(x => x.Key, x => new[] { x.Value }), decoderProfiles);
        }

        public static (Settings, DecoderProfile[]) ParseSettings(IDictionary<string, string[]> settingsRaw, IDictionary<string, Dictionary<string, string[]>> decoderProfilesRaw)
        {
            var paramReader = ParameterReader.Build();
            var settings = ParameterReader.Build().GetParameters<Settings>(settingsRaw);

            var decoderProfiles = decoderProfilesRaw.Any()
                ? decoderProfilesRaw
                    .Select(profileSection =>
                    {
                        var isDefaultFlacSection = profileSection.Key.Equals($"{ApplicationSettings.DecoderSectionPrefix}.FLAC", StringComparison.InvariantCultureIgnoreCase);

                        var profileRaw = isDefaultFlacSection
                            ? paramReader.GetParameters<DecoderProfileTempDefaultFlac>(profileSection.Value)
                            : paramReader.GetParameters<DecoderProfileTemp>(profileSection.Value);

                        return new DecoderProfile
                        {
                            Name = profileSection.Key.Replace($"{ApplicationSettings.DecoderSectionPrefix}.", "", StringComparison.InvariantCultureIgnoreCase),
                            Decoder = AudioDecoder.ResolveDecoderOrThrow(profileRaw.Decoder),
                            DecoderParameters = profileRaw.DecoderParameters,
                            TargetFileExtensions = profileRaw.TargetFileExtensions
                        };
                    }).ToArray()
                : new DecoderProfile[]
                {
                    GetDefaultFlacProfile(paramReader)
                };

            return (settings, decoderProfiles);
        }

        static DecoderProfile GetDefaultFlacProfile(ParameterReader paramReader)
        {
            var profileRaw = paramReader.GetParameters<DecoderProfileTempDefaultFlac>(new Dictionary<string, string[]>());

            return new DecoderProfile
            {
                Name = "FLAC",
                Decoder = AudioDecoder.ResolveDecoderOrThrow(profileRaw.Decoder),
                DecoderParameters = profileRaw.DecoderParameters,
                TargetFileExtensions = profileRaw.TargetFileExtensions
            };
        }

        class DecoderProfileTemp
        {
            [Parameter(nameof(Decoder))]
            public virtual string Decoder { get; set; }

            [Parameter(nameof(DecoderParameters))]
            public virtual string[] DecoderParameters { get; set; }

            [Parameter(nameof(TargetFileExtensions))]
            public virtual string[] TargetFileExtensions { get; set; }
        }

        class DecoderProfileTempDefaultFlac : DecoderProfileTemp
        {
            [Parameter(nameof(Decoder))]
            [Optional(defaultValue: "flac.exe")]
            public override string Decoder { get; set; }

            [Parameter(nameof(DecoderParameters))]
            [Optional(defaultValue: new string[] {
                Audio.Flac.Parameters.Options.Decoder.Decode,
                Audio.Flac.Parameters.Options.Decoder.ReadFromStdIn
            })]
            public override string[] DecoderParameters { get; set; }

            [Parameter(nameof(TargetFileExtensions))]
            [Optional(defaultValue: "flac")]
            public override string[] TargetFileExtensions { get; set; }
        }
    }
}