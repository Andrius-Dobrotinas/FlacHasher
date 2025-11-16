using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andy.FlacHash.Application.Win
{
    internal class HelpUtil
    {
        public static StringBuilder GetHelpText()
        {
            var thisAssemply = System.Reflection.Assembly.GetExecutingAssembly();
            var helpText_decoder = Help.GetTextResource(thisAssemply, "help.txt").ReplaceLineEndings();

            // Decoder profile params
            var builder = new System.Text.StringBuilder(helpText_decoder);
            var decoderProfileProperties = typeof(SettingsFile.DecoderProfileIniSection).GetProperties()
                .Where(Andy.Cmd.Parameter.Metadata.IsParameter)
                .ToList();
            
            var decoderProfileConfigBuilder = new System.Text.StringBuilder();
            Help.PrintParameters<SettingsFile.DecoderProfileIniSection, Andy.Cmd.Parameter.ParameterAttribute>(decoderProfileConfigBuilder, decoderProfileProperties, Array.Empty<System.Reflection.PropertyInfo>());
            builder.Replace("{DECODER_PROFILE_CONFIG}", decoderProfileConfigBuilder.ToString());
            decoderProfileConfigBuilder.Clear();

            // Decoder shared stuff
            var (decoderProperties, opSpecificProperties, miscProperties) = Help.GetPropertiesByParameterPurpose<Settings>();
            var decoderParamsLine = Help.GetParameterString<Settings, IniEntryAttribute>(decoderProperties, Array.Empty<System.Reflection.PropertyInfo>());
            builder.Replace("{DECODER_CONFIG}", decoderParamsLine);

            Help.PrintParameters<Settings, IniEntryAttribute>(decoderProfileConfigBuilder, opSpecificProperties, miscProperties);

            builder.Replace("{OTHER_SETTINGS}", decoderProfileConfigBuilder.ToString());
            builder.Replace("{SETTINGS_FILE_NAME}", Program.settingsFileName);

            return builder;
        }
    }
}
