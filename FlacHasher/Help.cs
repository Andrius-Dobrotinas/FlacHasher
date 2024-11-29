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
        public static void PrintParameters<T>(Action<string> writeUserLine)
        {
            var properties = Metadata.GetAllParameterMetadata<T>();
            var paramterGroups = Metadata.GetAllParameterGroups<T>();
            var withDependencies = Metadata.GetDependencyDictionary(properties);

            var sb = new StringBuilder();

            foreach (var group in paramterGroups)
            {
                Indent(sb, 1);
                sb.Append($"- [{group.Key.Item2}] -- {GetGroupingDescription(group.Key.Item1)}: ");
                
                sb.Append(string.Join(", ", group.Select(item => $"{properties[item].Sources.OrderBy(x => x.Order).First(x => x is CmdLineParameterAttribute).Name}")));

                writeUserLine(sb.ToString());
                sb.Clear();
            }

            writeUserLine("\nParameters:\n");


            var cmdlineParams = properties.Where(x => x.Value.Sources.Any(x => x is CmdLineParameterAttribute));
            var importantCmdlineParams = cmdlineParams.Where(x => x.Key.GetCustomAttribute<FrontAndCenterParamAttribute>() != null).ToList();
            foreach (var (property, metadata) in importantCmdlineParams.OrderBy(x => x.Value.Optionality))//.ThenBy(x => x.Value.DisplayName)
                PrintParameterDetails<CmdLineParameterAttribute>(sb, property, metadata, withDependencies, true, 1, writeUserLine);

            writeUserLine("\nLess important parameters:\n");
            foreach (var (property, metadata) in cmdlineParams.Except(importantCmdlineParams).OrderBy(x => x.Value.Optionality))//.ThenBy(x => x.Value.DisplayName)
                PrintParameterDetails<CmdLineParameterAttribute>(sb, property, metadata, withDependencies, true, 1, writeUserLine);

            writeUserLine("\nSettings file keys:\n");

            var settingsFileKeys = properties.Except(cmdlineParams).Where(x => x.Value.Sources.Any(x => x is IniEntryAttribute));
            var importantSettingsFileKeys = settingsFileKeys.Where(x => x.Key.GetCustomAttribute<FrontAndCenterParamAttribute>() != null).ToList();
            foreach (var (property, metadata) in importantSettingsFileKeys.Where(x => x.Value.Sources.Any(x => x is IniEntryAttribute)).OrderBy(x => x.Value.Optionality))//.ThenBy(x => x.Value.DisplayName)
            {
                Indent(sb, 1);
                sb.Append("* ");
                PrintParameterDetails<IniEntryAttribute>(sb, property, metadata, withDependencies, true, 0, writeUserLine);
            }

            writeUserLine("\nLess important:\n");
            foreach (var (property, metadata) in settingsFileKeys.Except(importantSettingsFileKeys).Where(x => x.Value.Sources.Any(x => x is IniEntryAttribute)).OrderBy(x => x.Value.Optionality))//.ThenBy(x => x.Value.DisplayName)
            {
                Indent(sb, 1);
                sb.Append("* ");
                PrintParameterDetails<IniEntryAttribute>(sb, property, metadata, withDependencies, true, 0, writeUserLine);
            }
        }

        static void PrintParameterDetails<TParam>(StringBuilder sb, PropertyInfo property, ParameterMetadata metadata, Dictionary<PropertyInfo, ParameterMetadata[]> withDependencies, bool showOptionality, int baseIndentationLevel, Action<string> writeUserLine)
            where TParam : ParameterAttribute
        {
            Indent(sb, baseIndentationLevel);
            var cmdParam = metadata.Sources.FirstOrDefault(x => x is TParam);
            if (cmdParam == null) return;

            sb.Append($"{cmdParam.Name} ");
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
                sb.Append($"-- {metadata.Description}");

            if (typeof(TParam) != typeof(IniEntryAttribute))
            {
                var cfgFileKey = metadata.Sources.FirstOrDefault(x => x is IniEntryAttribute);
                if (cfgFileKey != null)
                    sb.Append($" | Settings file key: \"{cfgFileKey.Name}\"");
            }

            if (withDependencies.ContainsKey(property))
            {
                var dependendyString = string.Join(", ", withDependencies[property]
                    .Select(x => $"\"{(x.Sources.FirstOrDefault(i => i is TParam) ?? x.Sources.First()).Name}\""));

                sb.Append($" | Requires: {dependendyString}");
            }

            writeUserLine(sb.ToString());
            sb.Clear();
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
    }
}
