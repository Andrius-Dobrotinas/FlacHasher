﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public static class Metadata
    {
        public static bool IsParameter(PropertyInfo property) => property.GetCustomAttributes<ParameterAttribute>(false).Any();

        public static Dictionary<PropertyInfo, ParameterMetadata> GetAllParameterMetadata<TParams, TParamSource>()
            where TParamSource : ParameterAttribute
        {
            var allProperties = typeof(TParams).GetProperties();
            return allProperties
                .Where(IsParameter)
                .ToDictionary(
                    property => property,
                    property => GetParameterMetadata<TParamSource>(property, allProperties));
        }

        public static ParameterMetadata GetParameterMetadata<TParamSource>(PropertyInfo property, ICollection<PropertyInfo> allProperties)
            where TParamSource : ParameterAttribute
        {
            var descrAttr = property.GetCustomAttribute<ParameterDescriptionAttribute>(false);
            var optionalAttrs = property.GetCustomAttributes<OptionalAttribute>(false).ToArray();

            var reqWith = property.GetCustomAttributes<RequiredWithAttribute>(false);
            var dependencyProperties = reqWith.Select(x => x.OtherPropertyName).ToArray();

            var isTrulyOptional = optionalAttrs.Any(x => x.GetType() == typeof(OptionalAttribute)); // only "optional" comes with no strings attached
            var isConditional = optionalAttrs.Any(x => x.GetType() != typeof(OptionalAttribute)); // derivatives of "optional", assume to be conditional
            var optionalWithDefaultValueAttr = optionalAttrs.FirstOrDefault(x => x.DefaultValue != null);

            return new ParameterMetadata
            {
                Property = property,
                DisplayName = descrAttr?.Name ?? property.Name,
                Description = descrAttr?.Description,
                Optionality = !isTrulyOptional && !isConditional 
                    ? OptionalityMode.Mandatory
                    : isConditional
                        ? OptionalityMode.Conditional
                        : OptionalityMode.Optional,
                EmptyAllowed = property.GetCustomAttribute<AllowEmptyAttribute>() != null,
                DefaultValue = optionalWithDefaultValueAttr?.DefaultValue,
                Sources = property.GetCustomAttributes<ParameterAttribute>(false).Where(source => source is TParamSource).ToList(),
                RequiredWith = reqWith.Any() ? allProperties.Where(x => dependencyProperties.Contains(x.Name)).ToArray() : null
            };
        }

        public static ParameterMetadata GetParameterMetadata<TParamSource>(Type paramsType, PropertyInfo property)
            where TParamSource : ParameterAttribute
        {
            var allProperties = paramsType.GetProperties().Where(IsParameter).ToArray();
            return GetParameterMetadata<TParamSource>(property, allProperties);
        }

        public static IGrouping<(Type, string), PropertyInfo>[] GetAllParameterGroups<TParams>()
        {
            var allProperties = typeof(TParams).GetProperties().Where(IsParameter).ToArray();
            return GetAllParameterGroups(allProperties);
        }

        public static IGrouping<(Type, string), PropertyInfo>[] GetAllParameterGroups(ICollection<PropertyInfo> allProperties)
        {
            var eitherOrGroups = allProperties.SelectMany(x => x.GetCustomAttributes<EitherOrAttribute>(false)
                .Select(i => new { Type = i.GetType(), Group = i.GroupKey, Property = x }));

            var atLeastOneGroups = allProperties.SelectMany(x => x.GetCustomAttributes<AtLeastOneOfAttribute>(false)
                .Select(i => new { Type = i.GetType(), Group = i.GroupKey, Property = x }));

            return eitherOrGroups.Concat(atLeastOneGroups).GroupBy(x => (x.Type, x.Group), x => x.Property)
                .ToArray();
        }

        public static Dictionary<PropertyInfo, ParameterMetadata[]> GetDependencyDictionary(Dictionary<PropertyInfo, ParameterMetadata> properties)
        {
            var conditionallyRequired = properties.Where(x => x.Value.RequiredWith != null);
            return conditionallyRequired
                .SelectMany(
                    x => x.Value.RequiredWith.Select(i => (i, x.Value)))
                .GroupBy(x => x.Item1, x => x.Value)
                .ToDictionary(x => x.Key, x => x.ToArray());
        }
    }
}
