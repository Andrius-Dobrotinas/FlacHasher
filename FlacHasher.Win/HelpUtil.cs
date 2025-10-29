using System;
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
            var helpText = Help.GetHelpText().ReplaceLineEndings();
            var helpHashfileText = Help.GetHashfileHelpText().ReplaceLineEndings();
            var thisAssemply = System.Reflection.Assembly.GetExecutingAssembly();
            var helpApplicationSpecific = Help.GetTextResource(thisAssemply, "help.txt").ReplaceLineEndings();
            var helpText_decoder = Help.GetTextResource(thisAssemply, "help_decoder_config.txt").ReplaceLineEndings();

            var builder = new System.Text.StringBuilder(helpText);
            var builderApplicationSpecific = new System.Text.StringBuilder(helpApplicationSpecific);

            builderApplicationSpecific.Replace(Help.Placeholder.HashfileDescription, helpHashfileText);
            builder.Replace(Help.Placeholder.ApplicationSpecific, builderApplicationSpecific.ToString());

            // Decoder profile params
            var decoderParamsBuilder = new System.Text.StringBuilder(helpText_decoder);
            var decoderProfileProperties = typeof(SettingsFile.DecoderProfileTemp).GetProperties().Where(Andy.Cmd.Parameter.Metadata.IsParameter);
            var cmdlineProperties = typeof(Cmd.MasterParameters).GetProperties().Where(Andy.Cmd.Parameter.Metadata.IsParameter)
                .Where(x => decoderProfileProperties.Contains(x, Application.Help.PropertyInfoNameComparer.Instance))
                .ToList();
            var temp = new System.Text.StringBuilder();

            builder.Replace(Help.Placeholder.DecoderSection, Help.GetDecoderSectionText().ReplaceLineEndings());

            Help.PrintParameters<Cmd.MasterParameters, IniEntryAttribute>(temp, cmdlineProperties, Array.Empty<System.Reflection.PropertyInfo>());
            decoderParamsBuilder.Replace("{DECODER_PROFILE_CONFIG}", temp.ToString());

            // Decoder shared stuff
            var (decoderProperties, opSpecificProperties, miscProperties) = Help.GetPropertiesByParameterPurpose<Settings>();
            var decoderParamsLine = Help.GetParameterString<Settings, IniEntryAttribute>(decoderProperties, Array.Empty<System.Reflection.PropertyInfo>());
            decoderParamsBuilder.Replace("{DECODER_CONFIG}", decoderParamsLine);

            builder.Replace(Help.Placeholder.DecoderParams, decoderParamsBuilder.ToString());

            temp.Clear();
            Help.PrintParameters<Settings, IniEntryAttribute>(temp, opSpecificProperties, miscProperties);

            builder.Replace("{OTHER_SETTINGS}", temp.ToString());
            builder.Replace("{SETTINGS_FILE_NAME}", Program.settingsFileName);

            return builder;
        }
    }
}
