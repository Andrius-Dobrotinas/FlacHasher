using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Andy.Cmd.Parameter;

namespace Andy.FlacHash.Application
{
    public static class Help
    {
        public const int ParamterColumnLength = 50;
        public const string HeadingIndentation = "  ";

        public static string GetParameterString<T>(
            ICollection<PropertyInfo> propertiesToExclude)
        {
            var builder = new StringBuilder();

            var allProperties = typeof(T).GetProperties().Where(Metadata.IsParameter);

            var allOperationSpecificProperties = allProperties.Except(propertiesToExclude, PropertyInfoComparer.Instance).ToList();

            var allOperationSpecificProperties_Main = allOperationSpecificProperties.Where(x => x.GetCustomAttribute<OperationParamAttribute>() != null).ToList();
            var allOperationSpecificProperties_Other = allOperationSpecificProperties.Except(allOperationSpecificProperties_Main).ToList();

            PrintParameters<T>(builder, allOperationSpecificProperties_Main, allOperationSpecificProperties_Other);

            return builder.ToString();
        }

        public static string GetParameterString<TMaster>(ICollection<PropertyInfo> mainParameterProperties, ICollection<PropertyInfo> otherParameterProperties)
        {
            var sb = new StringBuilder();

            PrintParameters<TMaster>(sb, mainParameterProperties, otherParameterProperties);

            return sb.ToString();
        }

        public static void PrintParameters<T>(
            StringBuilder builder,
            ICollection<PropertyInfo> mainParameterProperties,
            ICollection<PropertyInfo> otherParameterProperties)
        {
            var allParams = Metadata.GetAllParameterMetadata<T>();
            var paramterGroups = Metadata.GetAllParameterGroups([.. mainParameterProperties, .. otherParameterProperties]);
            var dependencyMap = Metadata.GetDependencyDictionary(allParams);

            var groupStringBuilder = new StringBuilder();

            foreach (var group in paramterGroups)
            {
                Indent(groupStringBuilder, 1);
                groupStringBuilder.Append($"- [{group.Key.Item2}] -- {GetGroupingDescription(group.Key.Item1)}: ");
                
                groupStringBuilder.AppendLine(string.Join(", ", group.Select(item => $"{allParams[item].Sources.OrderBy(x => x.Order).First(x => x is CmdLineParameterAttribute).Name}")));

                builder.Append(groupStringBuilder.ToString());
                groupStringBuilder.Clear();
            }

            if (paramterGroups.Any())
                builder.Append(Environment.NewLine);

            var mainOperationParams = allParams.Where(x => mainParameterProperties.Contains(x.Key, PropertyInfoComparer.Instance)).ToDictionary(x => x.Key, x => x.Value);
            PrintParameters(builder, mainOperationParams, dependencyMap);

            var otherOperationParams = allParams.Where(x => otherParameterProperties.Contains(x.Key, PropertyInfoComparer.Instance)).ToDictionary(x => x.Key, x => x.Value);
            if (otherOperationParams.Any())
            {
                builder.AppendLine($"{Environment.NewLine}{HeadingIndentation}Misc configuration:");
                PrintParameters(builder, otherOperationParams, dependencyMap);
            }
        }

        static void PrintParameters(StringBuilder sb, Dictionary<PropertyInfo, ParameterMetadata> allParams, Dictionary<PropertyInfo, ParameterMetadata[]> dependencyMap)
        {
            // Table heading
            Indent(sb, 1);
            sb.Append("[ --cmd-line Parameter | Settings file Key ]".PadRight(ParamterColumnLength));
            sb.AppendLine("[ Description ]");

            var cmdlineParams = allParams.Where(x => x.Value.Sources.Any(x => x is CmdLineParameterAttribute));
            var importantCmdlineParams = cmdlineParams.Where(x => x.Key.GetCustomAttribute<FrontAndCenterParamAttribute>() != null).ToList();
            foreach (var (property, metadata) in importantCmdlineParams.OrderBy(x => x.Value.Optionality))
                PrintParameterDetails<CmdLineParameterAttribute>(sb, property, metadata, dependencyMap, true, 1);

            var cmdlineParams_lessImportant = cmdlineParams.Except(importantCmdlineParams);
            if (cmdlineParams_lessImportant.Any())
            {
                if (importantCmdlineParams.Any())
                    sb.AppendLine($"{Environment.NewLine}{HeadingIndentation}Less important:");

                foreach (var (property, metadata) in cmdlineParams_lessImportant.OrderBy(x => x.Value.Optionality))
                    PrintParameterDetails<CmdLineParameterAttribute>(sb, property, metadata, dependencyMap, true, 1);
            }

            var settingsFileKeys = allParams.Except(cmdlineParams).Where(x => x.Value.Sources.Any(x => x is IniEntryAttribute));
            if (settingsFileKeys.Any())
            {
                var importantSettingsFileKeys = settingsFileKeys.Where(x => x.Key.GetCustomAttribute<FrontAndCenterParamAttribute>() != null).ToList();
                foreach (var (property, metadata) in importantSettingsFileKeys.Where(x => x.Value.Sources.Any(x => x is IniEntryAttribute)).OrderBy(x => x.Value.Optionality).ThenBy(x => x.Value.DisplayName))
                {
                    PrintParameterDetails<IniEntryAttribute>(sb, property, metadata, dependencyMap, true, 1, "* ");
                }

                foreach (var (property, metadata) in settingsFileKeys.Except(importantSettingsFileKeys).Where(x => x.Value.Sources.Any(x => x is IniEntryAttribute)).OrderBy(x => x.Value.Optionality).ThenBy(x => x.Value.DisplayName))
                {
                    PrintParameterDetails<IniEntryAttribute>(sb, property, metadata, dependencyMap, true, 1, "* ");
                }
            }
        }

        static void PrintParameterDetails<TParam>(StringBuilder sb, PropertyInfo property, ParameterMetadata metadata, Dictionary<PropertyInfo, ParameterMetadata[]> withDependencies, bool showOptionality, int baseIndentationLevel, string prefix = "")
            where TParam : ParameterAttribute
        {
            Indent(sb, baseIndentationLevel);

            var cmdParam = metadata.Sources.FirstOrDefault(x => x is TParam);
            if (cmdParam == null) return;
            var str = $"{prefix}{cmdParam.Name} ";

            if (typeof(TParam) != typeof(IniEntryAttribute))
            {
                var cfgFileKey = metadata.Sources.FirstOrDefault(x => x is IniEntryAttribute);
                if (cfgFileKey != null)
                    str = $"{str}| {cfgFileKey.Name}";
            }

            sb.Append(str.PadRight(ParamterColumnLength));

            if (metadata.Optionality != OptionalityMode.Mandatory)
            {
                var defaultValue = metadata.DefaultValue == null
                    ? null
                    : metadata.DefaultValue is Array
                        ? "[ " + string.Join("; ", (metadata.DefaultValue as IList<string>).Select(x => $"\"{x}\"")) + " ]"
                        : $"\"{metadata.DefaultValue}\"";

                if (showOptionality)
                {
                    sb.Append($"[{metadata.Optionality}");

                    if (defaultValue != null)
                        sb.Append($", default value: {defaultValue}");

                    sb.Append("] ");
                }
                else
                {
                    if (defaultValue != null)
                        sb.Append($" [default value: {defaultValue}] ");
                }
            }

            if (metadata.EmptyAllowed)
                sb.Append($"[Empty value allowed] ");

            if (!string.IsNullOrEmpty(metadata.Description))
                sb.Append($"{metadata.Description}");

            if (withDependencies.ContainsKey(property))
            {
                var dependendyString = string.Join(", ", withDependencies[property]
                    .Select(x => $"\"{(x.Sources.FirstOrDefault(i => i is TParam) ?? x.Sources.First()).Name}\""));

                sb.Append($" | Requires: {dependendyString}");
            }
            sb.AppendLine();
        }

        static void Indent(StringBuilder sb, int level)
        {
            for (int i = 0; i < level; i++)
                sb.Append("  ");
        }

        static string GetGroupingDescription(Type type)
        {
            if (type == typeof(AtLeastOneOfAttribute))
                return "At least one of the following must have a value";

            if (type == typeof(EitherOrAttribute))
                return "One of the following must have a value";

            if (type == typeof(OptionalEitherOrAttribute))
                return "Optional: no more than one of the following must have a value";

            return "";
        }

        public static (ICollection<PropertyInfo> sharedDecoderProperties, ICollection<PropertyInfo> sharedOpSpecificProperties, ICollection<PropertyInfo> sharedMiscProperties) GetPropertiesByParameterPurpose<TParams>()
        {
            var sharedParamProperties = typeof(TParams).GetProperties().Where(Metadata.IsParameter);

            var sharedDecoderProperties = sharedParamProperties.Where(x => x.GetCustomAttribute<DecoderParamAttribute>() != null).ToList();
            var sharedOpSpecificProperties = sharedParamProperties.Except(sharedDecoderProperties).ToList();
            var sharedMiscProperties = sharedParamProperties.Except(sharedDecoderProperties).Except(sharedOpSpecificProperties).ToList();

            return (sharedDecoderProperties, sharedOpSpecificProperties, sharedMiscProperties);
        }

        public class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
        {
            public readonly static PropertyInfoComparer Instance = new PropertyInfoComparer();

            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                if (x == y) return true;
                if (x == null || y == null) return false;

                return x.Name == y.Name && x.DeclaringType == y.DeclaringType;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return 0;
            }
        }
    }
}