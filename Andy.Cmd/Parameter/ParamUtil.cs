using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public class ParamUtil
    {
        public static T CreateWithDefaults<T>()
    where T : class, new()
        {
            var settings = new T();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var optionalAttr = property.GetCustomAttribute<OptionalAttribute>();
                if (optionalAttr?.DefaultValue != null)
                {
                    if (property.PropertyType.IsArray)
                        if (property.PropertyType.GetElementType() != typeof(string))
                            throw new NotSupportedException("For arrays, only string type is allowed");
                        else
                        {
                            var value = optionalAttr.DefaultValue is string[]? (string[])optionalAttr.DefaultValue
                                : ((string)optionalAttr.DefaultValue).Split(ParameterValueResolver.ArrayValueSeparator);
                            property.SetValue(settings, value);
                        }
                    else
                        property.SetValue(settings, optionalAttr.DefaultValue);
                }
            }

            return settings;
        }
    }
}
