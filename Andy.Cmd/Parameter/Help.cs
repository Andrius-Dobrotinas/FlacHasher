using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public static class Help
    {
        public static Dictionary<PropertyInfo, ParameterDescription> GetAllParameterMetadata<TParams>()
        {
            var allProperties = typeof(TParams).GetProperties();
            return allProperties
                .ToDictionary(
                    property => property,
                    property => GetParameterMetadata(property, allProperties));
        }

        public static ParameterDescription GetParameterMetadata(PropertyInfo property, ICollection<PropertyInfo> allProperties)
        {
            var descrAttr = property.GetCustomAttribute<ParameterDescriptionAttribute>(false);
            var attrs = property.GetCustomAttributes<ParameterAttribute>(false).ToArray();
            var optionalAttrs = property.GetCustomAttributes<OptionalAttribute>(false).ToArray();

            var reqWith = property.GetCustomAttribute<RequiredWithAttribute>();

            var isTrulyOptional = optionalAttrs.Any(x => x.GetType() == typeof(OptionalAttribute));
            var isConditional = optionalAttrs.Any(x => x.GetType() != typeof(OptionalAttribute));
            return new ParameterDescription
            {
                Property = property,
                DisplayName = descrAttr?.Name ?? property.Name,
                Description = descrAttr?.Description,
                Optionality = !isTrulyOptional && !isConditional 
                    ? OptionalityMode.Mandatory
                    : isConditional
                        ? OptionalityMode.Conditional
                        : OptionalityMode.Optional,
                EmptyAllowed = property.GetCustomAttribute<RequiredWithAttribute>() != null,
                Sources = attrs.OrderBy(x => x.Order).Select(paramAttr =>
                {
                    var sourceDisplayName = paramAttr.GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? paramAttr.GetType().Name;
                    return new KeyValuePair<string, string>(paramAttr.Name, sourceDisplayName);
                }).ToArray(),
                RequiredWith = reqWith == null ? null : allProperties.FirstOrDefault(x => x.Name == reqWith.OtherPropertyName)
            };
        }

        public static ParameterDescription GetParameterMetadata(Type paramsType, PropertyInfo property)
        {
            var allProperties = paramsType.GetProperties();
            return GetParameterMetadata(property, allProperties);
        }

        public static IGrouping<(Type, string), PropertyInfo>[] GetAllParameterGroups<TParams>()
        {
            var allProperties = typeof(TParams).GetProperties();
            var eitherOrGroups = allProperties.SelectMany(x => x.GetCustomAttributes<EitherOrAttribute>(false)
                .Select(i => new { Type = i.GetType(), Group = i.GroupKey, Property = x }));

            var atLeastOneGroups = allProperties.SelectMany(x => x.GetCustomAttributes<AtLeastOneOfAttribute>(false)
                .Select(i => new { Type = i.GetType(), Group = i.GroupKey, Property = x }));

            return eitherOrGroups.Concat(atLeastOneGroups).GroupBy(x => (x.Type, x.Group), x => x.Property)
                .ToArray();
        }
    }
}
