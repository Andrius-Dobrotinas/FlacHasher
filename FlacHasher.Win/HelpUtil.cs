﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var decoderProfileProperties = typeof(SettingsFile.DecoderProfileTemp).GetProperties().Where(Andy.Cmd.Parameter.Metadata.IsParameter);
            var cmdlineProperties = typeof(Cmd.MasterParameters).GetProperties().Where(Andy.Cmd.Parameter.Metadata.IsParameter)
                .Where(x => decoderProfileProperties.Contains(x, Application.Help.PropertyInfoNameComparer.Instance))
                .ToList();
            
            var decoderProfileConfigBuilder = new System.Text.StringBuilder();
            Help.PrintParameters<Cmd.MasterParameters, IniEntryAttribute>(decoderProfileConfigBuilder, cmdlineProperties, Array.Empty<System.Reflection.PropertyInfo>());
            builder.Replace("{DECODER_PROFILE_CONFIG}", decoderProfileConfigBuilder.ToString());

            // Decoder shared stuff
            var (decoderProperties, opSpecificProperties, miscProperties) = Help.GetPropertiesByParameterPurpose<Settings>();
            var decoderParamsLine = Help.GetParameterString<Settings, IniEntryAttribute>(decoderProperties, Array.Empty<System.Reflection.PropertyInfo>());
            builder.Replace("{DECODER_CONFIG}", decoderParamsLine);

            decoderProfileConfigBuilder.Clear();
            Help.PrintParameters<Settings, IniEntryAttribute>(decoderProfileConfigBuilder, opSpecificProperties, miscProperties);

            builder.Replace("{OTHER_SETTINGS}", decoderProfileConfigBuilder.ToString());
            builder.Replace("{SETTINGS_FILE_NAME}", Program.settingsFileName);

            return builder;
        }
    }
}
