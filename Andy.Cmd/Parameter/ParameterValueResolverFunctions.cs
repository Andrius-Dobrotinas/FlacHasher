using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    internal class ParameterValueResolverFunctions
    {
        internal static bool IsNullablePrimitiveType(Type type)
        {
            return IsNullableValueType(type) && type.GenericTypeArguments.Single().IsPrimitive;
        }

        /// <summary>
        /// Tells whether a non-null string is empty or whitespace.
        /// If it's null, the answer is False as it's not technically an empty string or whitespace.
        /// </summary>
        internal static bool IsEmptyOrWhitespace(string value) => 
            value != null && string.IsNullOrWhiteSpace(value);

        internal static bool IsNullableValueType(Type type)
        {
            return type.IsValueType && type.IsGenericType && type.GenericTypeArguments.Length == 1;
        }

        internal static string[] TrimValues(IEnumerable<string> values) =>
            values.Select(x => x?.Trim()).ToArray();

        internal static void SetArrayValueTrimmed<TParams>(TParams paramsInstances, PropertyInfo property, string paramName, string[] values)
        {
            var valuesTrimmed = TrimValues(values);
            if (valuesTrimmed.Any(string.IsNullOrEmpty))
                throw new BadParameterValueException(paramName, "An array is not allowed to contain empty elements");

            property.SetValue(paramsInstances, valuesTrimmed);
        }

        internal static object ParseNonNullPrimitive(string value, Type type)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (type.IsPrimitive)
            {
                if (type == typeof(bool))
                {
                    if (value == "1")
                        return true;
                    else if (value == "0")
                        return false;
                }
                return Convert.ChangeType(value, type);
            }
            else if (IsNullablePrimitiveType(type))
            {
                var actualType = type.GenericTypeArguments.SingleOrDefault() ?? throw new NotSupportedException($"Expected a nullable value type to only have one generic type parameter: {type.FullName}");
                if (!actualType.IsPrimitive)
                    throw new NotImplementedException("Non-primitive value type");

                return ParseNonNullPrimitive(value, actualType);
            }

            throw new NotSupportedException(type.FullName);
        }

        internal static bool TryGetFirstPresentValue(IDictionary<string, string[]> arguments, IEnumerable<string> paramNames, out string foundParamName, out string[] values)
        {
            foreach (var param in paramNames)
            {
                var argExists = arguments.TryGetValue(param, out values);
                if (argExists)
                {
                    foundParamName = param;
                    return true;
                }
            }

            foundParamName = null;
            values = null;
            return false;
        }
    }
}