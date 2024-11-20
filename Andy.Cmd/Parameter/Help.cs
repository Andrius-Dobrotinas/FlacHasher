using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public static class Help
    {
        public static IEnumerable<ParameterDescription> GetParameterMetadata<Type>()
        {
            var allProperties = typeof(Type).GetProperties();

            foreach (var property in allProperties)
            {
                var descrAttr = property.GetCustomAttribute<ParameterDescriptionAttribute>(false);
                var attrs = property.GetCustomAttributes<ParameterAttribute>(false).ToArray();
                var optionalAttrs = property.GetCustomAttributes<OptionalAttribute>(false).ToArray();

                var reqWith = property.GetCustomAttribute<RequiredWithAttribute>();

                yield return new ParameterDescription
                {
                    Property = property,
                    DisplayName = descrAttr?.Name ?? property.Name,
                    Description = descrAttr?.Description,
                    IsOptional = optionalAttrs.Any(),
                    Sources = attrs.OrderBy(x => x.Order).Select(paramAttr =>
                    {
                        var sourceDisplayName = paramAttr.GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? paramAttr.GetType().Name;
                        return new KeyValuePair<string, string>(paramAttr.Name, sourceDisplayName);
                    }).ToArray(),
                    RequiredWith = reqWith == null ? null : allProperties.FirstOrDefault(x => x.Name == reqWith.OtherPropertyName)
                };
            }
        }
    }
}
