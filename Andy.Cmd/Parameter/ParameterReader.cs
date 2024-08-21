using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public class ParameterReader
    {
        public const char ArrayValueSeparator = ';';

        /// <summary>
        /// String parameters have null values only if they're not specified; otherwise, it's at least an empty string (if allowed).
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static TParam GetParameters<TParam>(IDictionary<string, string> arguments)
            where TParam : new()
        {
            var properties = typeof(TParam).GetProperties();

            var paramNames = properties.Select(p => p.GetCustomAttribute<ParameterAttribute>())
                .Where(attr => attr != null)
                .Select(attr => attr.Name)
                .ToList();

            if (paramNames.Distinct().Count() != paramNames.Count)
                throw new InvalidOperationException("Some parameter names are repeated");

            var @params = new TParam();
            foreach (var property in properties)
            {
                Parse(property, arguments, @params);
            }

            return @params;
        }

        static bool IsNullableValueType(Type type)
        {
            return type.IsValueType && type.IsGenericType && type.GenericTypeArguments.Length == 1;
        }

        static bool IsNullablePrimitiveType(Type type)
        {
            return IsNullableValueType(type) && type.GenericTypeArguments.Single().IsPrimitive;
        }

        public static void Parse<T>(PropertyInfo property, IDictionary<string, string> arguments, T paramsInstances)
        {
            var paramAttr = property.GetCustomAttributes(typeof(ParameterAttribute), false).SingleOrDefault() as ParameterAttribute;
            if (paramAttr == null)
                return;

            var propertyType = property.PropertyType;
            if (!(propertyType == typeof(string)
                || propertyType.IsPrimitive 
                || IsNullablePrimitiveType(propertyType)
                || (propertyType.IsArray && propertyType.HasElementType)))
                throw new NotSupportedException($"Only primitive value types, strings and arrays strings have been implemented. Property: {property.Name}");

            var optionalAttr = property.GetCustomAttributes(typeof(OptionalAttribute), false).SingleOrDefault() as OptionalAttribute;
            var isOptional = optionalAttr != null;
            var isEmptyAllowed = property.GetCustomAttributes(typeof(AllowEmptyAttribute), false).SingleOrDefault() as AllowEmptyAttribute != null;

            if (isEmptyAllowed
                && !(propertyType == typeof(string) || propertyType.IsArray && isOptional))
                throw new NotSupportedException($"{nameof(AllowEmptyAttribute)} is only applicable to String and Optional Array type parameters. Property: {property.Name}");

            if (isOptional && optionalAttr.DefaultValue != null
                && propertyType.IsArray)
                throw new NotSupportedException($"Optional Array type parameters can't have default values - there's no good reason for that. Property: {property.Name}");

            if (propertyType.IsValueType)
            {
                HandleValueType(arguments, property, paramAttr, optionalAttr, paramsInstances);
            }
            else
            {
                if (propertyType == typeof(string))
                {
                    string value;
                    var argExists = arguments.TryGetValue(paramAttr.Name, out value);

                    if (!argExists)
                        if (isOptional)
                        {
                            if (optionalAttr.DefaultValue != null)
                                property.SetValue(paramsInstances, optionalAttr.DefaultValue);
                            return;
                        }
                        else
                            throw new ParameterMissingException(paramAttr.Name);
                    else
                    {
                        if (propertyType == typeof(string))
                        {
                            if (value == null || value == "" && !isEmptyAllowed)
                                throw new ParameterEmptyException(paramAttr.Name);
                            else
                                property.SetValue(paramsInstances, value);
                        }
                        else
                            throw new NotImplementedException($"Type: {propertyType.FullName}. Property: {property.Name}");
                    }
                }
                else if (propertyType.IsArray)
                {
                    if (!propertyType.HasElementType)
                        throw new NotSupportedException($"Array type without its element type: {property.Name} ({propertyType.FullName})");

                    var elementType = propertyType.GetElementType();
                    if (elementType != typeof(string))
                        throw new NotSupportedException($"Array of other than Strings is not supported: {property.Name}, {propertyType.FullName}");

                    string value;
                    var argExists = arguments.TryGetValue(paramAttr.Name, out value);

                    if (!argExists)
                        if (isOptional)
                            return;
                        else
                            throw new ParameterMissingException(paramAttr.Name);
                    else
                    {
                        if (value == null)
                            throw new ParameterEmptyException(paramAttr.Name);
                        else if (string.IsNullOrWhiteSpace(value))
                        {
                            if (!isEmptyAllowed)
                                throw new ParameterEmptyException(paramAttr.Name);
                            else
                            {
                                property.SetValue(paramsInstances, Array.Empty<string>());
                                return;
                            }
                        }
                        else
                        {
                            var split = value.Split(ArrayValueSeparator).ToArray();
                            if (split.Any(x => string.IsNullOrWhiteSpace(x)))
                                throw new BadParameterValueException(paramAttr.Name, "An array is not allowed to contain empty elements");

                            var arr = split.Cast<string>().ToArray();

                            property.SetValue(paramsInstances, arr);
                            return;
                        }
                    }
                }
                else
                    throw new NotSupportedException($"Not supported type: {property.Name} ({propertyType.FullName})");
            }
        }

        static void HandleValueType<TTarget>(IDictionary<string, string> arguments, PropertyInfo property, ParameterAttribute paramAttr, OptionalAttribute optionalAttr, TTarget paramsInstances)
        {
            var propertyType = property.PropertyType;
            var isOptional = optionalAttr != null;

            if (propertyType.IsPrimitive || IsNullablePrimitiveType(propertyType))
            {
                string value;
                var argExists = arguments.TryGetValue(paramAttr.Name, out value);

                if (!argExists)
                    if (isOptional)
                    {
                        if (optionalAttr.DefaultValue != null)
                            property.SetValue(paramsInstances, optionalAttr.DefaultValue);
                        return;
                    }
                    else if (IsNullablePrimitiveType(propertyType))
                    {
                        // keep the value null
                        return;
                    }
                    else
                        throw new ParameterMissingException(paramAttr.Name);
                else
                {
                    // If a boolean value is specified as a flag (ie without any value at all), it's True
                    if ((propertyType == typeof(bool) || propertyType == typeof(bool?))
                        && value == null)
                    {
                        property.SetValue(paramsInstances, true);
                        return;
                    }
                    else
                    {
                        // The parameter has been specified; it HAS to contain a value, otherwise it's no bueno
                        if (string.IsNullOrWhiteSpace(value))
                            throw new ParameterEmptyException(paramAttr.Name);

                        object parsedValue;
                        try
                        {
                            parsedValue = ParseNonNullPrimitive(value, property.PropertyType);
                        }
                        catch (FormatException e)
                        {
                            throw new BadParameterValueException(paramAttr.Name, e);
                        }

                        property.SetValue(paramsInstances, parsedValue);
                        return;
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"Non-primitive value types are not supported. Property: {property.Name} ({propertyType.FullName})");
            }
        }

        static object ParseNonNullPrimitive(string value, Type type)
        {
            if (type.IsPrimitive)
            {
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
    }
}