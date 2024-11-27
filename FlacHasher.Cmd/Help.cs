using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Andy.Cmd.Parameter;

namespace Andy.FlacHash.Application.Cmd
{
    internal static class Help
    {
        public static void PrintParameters<T>(Action<string> writeUserLine)
        {
            var properties = Metadata.GetAllParameterMetadata<T>();
            var paramterGroups = Metadata.GetAllParameterGroups<T>();
            var withDependencies = Metadata.GetDependencyDictionary(properties);

            var sb = new StringBuilder();

            foreach (var group in paramterGroups)
            {
                sb.Append($"- \"{group.Key.Item2}\" -- {GetGroupingDescription(group.Key.Item1)}: ");
                sb.AppendLine(string.Join(", ", group.Select(item => $"{properties[item].DisplayName}")));

                foreach (var property in group)
                {
                    var propertyMetadata = properties[property];
                    PrintParameterDetails(sb, property, propertyMetadata, withDependencies, false, 1, writeUserLine);
                };

                writeUserLine(sb.ToString());
                sb.Clear();
            }

            writeUserLine("");

            var doneProperties = paramterGroups.SelectMany(x => x).ToList();
            var unlistedProperties = properties.Where(x => !doneProperties.Contains(x.Key));
            foreach (var (property, metadata) in unlistedProperties.OrderBy(x => x.Value.Optionality))
            {
                PrintParameterDetails(sb, property, metadata, withDependencies, true, 0, writeUserLine);
            }
        }

        static void PrintParameterDetails(StringBuilder sb, PropertyInfo property, ParameterMetadata metadata, Dictionary<PropertyInfo, ParameterMetadata[]> withDependencies, bool showOptionality, int baseIndentationLevel, Action<string> writeUserLine)
        {
            Indent(sb, baseIndentationLevel);
            sb.Append($"- {metadata.DisplayName} ");
            if (metadata.Optionality != OptionalityMode.Mandatory)
            {
                var defaultValue = metadata.DefaultValue == null
                    ? null
                    : metadata.DefaultValue is Array
                        ? "[ " + string.Join("; ", (metadata.DefaultValue as IList<string>).Select(x => $"\"{x}\"")) + " ]"
                        : $"\"{metadata.DefaultValue}\"";

                if (showOptionality)
                {
                    sb.Append($" [{metadata.Optionality}");

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
                sb.Append($"[Empty value allowed]");

            if (!string.IsNullOrEmpty(metadata.Description))
                sb.Append($": {metadata.Description}");

            sb.AppendLine();
            Indent(sb, baseIndentationLevel + 2);
            sb.AppendLine($"Configured via:");

            foreach (var srcGrp in metadata.Sources.GroupBy(x => x.Value))
            {
                Indent(sb, baseIndentationLevel + 3);
                sb.Append($"* ");
                sb.Append(string.Join(", ", srcGrp.Select(x => x.Key)));

                sb.AppendLine($" ({srcGrp.Key})");
            }

            if (withDependencies.ContainsKey(property))
            {
                var dependendyString = string.Join(", ", withDependencies[property].Select(x => $"\"{x.DisplayName}\""));

                Indent(sb, baseIndentationLevel + 2);
                sb.AppendLine($"Requires: {dependendyString}");
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
                return "Strictly One of the following must have a value";

            if (type == typeof(OptionalEitherOrAttribute))
                return "Optional: no more than one of the following must have a value";

            return "";
        }
    }
}
