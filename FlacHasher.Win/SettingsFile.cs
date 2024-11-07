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

            var settings = Application.SettingsFile.GetSettingsProfile(wholeSettingsFileDictionary, profileName);
            Application.SettingsFile.MergeSectionValuesIn(settings, wholeSettingsFileDictionary, ApplicationSettings.DefaultHashingSection);

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
            if (!decoderProfilesRaw.Any())
                throw new ConfigurationException("At least one decoder profile must be defined");

            var paramReader = ParameterReader.Build();
            var settings = ParameterReader.Build().GetParameters<Settings>(settingsRaw);

            var decoderProfiles = decoderProfilesRaw
                .Select(profileSection => {
                    // Default Decoder section is for FLAC. I can insert file extension and decoder params in there, but I can't hardcode a path to the exe
                    var isDefaultFlacSection = profileSection.Key.Equals(ApplicationSettings.DefaultDecoderSection, StringComparison.InvariantCultureIgnoreCase);

                    var profileRaw = isDefaultFlacSection
                        ? paramReader.GetParameters<DecoderProfileTempDefaultFlac>(profileSection.Value)
                        : paramReader.GetParameters<DecoderProfileTemp>(profileSection.Value);

                    var decoderExe = AudioDecoder.ResolveDecoderOrThrow(profileRaw.Decoder);

                    return new DecoderProfile
                    {
                        Name = profileSection.Key.Replace("Decoder.", "", StringComparison.InvariantCultureIgnoreCase),
                        Decoder = decoderExe,
                        DecoderParameters = profileRaw.DecoderParameters,
                        TargetFileExtensions = profileRaw.TargetFileExtensions
                    };
                })
                .ToArray();

            return (settings, decoderProfiles);
        }

        class DecoderProfileTemp
        {
            [Parameter(nameof(Decoder))]
            public string Decoder { get; set; }

            [Parameter(nameof(DecoderParameters))]
            public virtual string[] DecoderParameters { get; set; }

            [Parameter(nameof(TargetFileExtensions))]
            public virtual string[] TargetFileExtensions { get; set; }
        }

        class DecoderProfileTempDefaultFlac : DecoderProfileTemp
        {
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