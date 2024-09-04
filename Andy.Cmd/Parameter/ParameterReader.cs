using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public class ParameterReader
    {
        public const char ArrayValueSeparator = ';';
        private static Type[] allowedTypes = new Type[] { typeof(string), typeof(string[]) };

        /// <summary>
        /// String parameters have null values only if they're not specified; otherwise, it's at least an empty string (if allowed).
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static TParam GetParameters<TParam>(IDictionary<string, string[]> arguments)
            where TParam : new()
        {
            var properties = typeof(TParam).GetProperties();

            var paramNames = properties.Select(p => p.GetCustomAttribute<ParameterAttribute>())
                .Where(attr => attr != null)
                .Select(attr => attr.Name)
                .ToList();

            if (paramNames.Distinct().Count() != paramNames.Count)
                throw new InvalidOperationException("Some parameter names are repeated");

            // Either-Or attribute
            var eitherOrProperties = properties.Select(property => new { property, attr = property.GetCustomAttribute<EitherOrAttribute>() })
                .Where(x => x.attr != null)
                .ToList();
            
            // In theory, other types could be accepted, but I don't need that now
            if (eitherOrProperties.Any(x => !allowedTypes.Contains(x.property.PropertyType)))
                throw new InvalidOperationException($"{nameof(EitherOrAttribute)} is only allowed on String and Array-of-String type of properties");

            var eitherOrPropertyGroups = eitherOrProperties
                .GroupBy(x => x.attr.GroupKey, x => x.property)
                .ToDictionary(x => x.Key, x => x.ToArray());

            if (eitherOrPropertyGroups.Any(group => group.Value.Length == 1))
            {
                var propertyRepresentations = eitherOrPropertyGroups
                    .Where(group => group.Value.Length == 1)
                    .Select(x => $"{x.Value.First().Name} (key: \"{x.Key}\")");
                throw new InvalidOperationException($"The following properties don't have any counterparts marked with the same {nameof(EitherOrAttribute)} key: {string.Join(',', propertyRepresentations)}");
            }

            var @params = new TParam();
            foreach (var property in properties)
            {
                ReadParameter(property, arguments, @params);
            }

            // Either-Or Continued
            foreach (var parameterGroup in eitherOrPropertyGroups)
            {
                var values = parameterGroup.Value.Select(x => x.GetValue(@params));
                var nullValues = values.Select(x => x == null);
                if (nullValues.Count(x => x == false) > 1)
                    throw new ParameterGroupException("Only one parameter is allowed to have a value", GetParameterNames(parameterGroup.Value));

                if (nullValues.All(x => x == true))
                    throw new ParameterGroupException("One of the following parameter must have a value", GetParameterNames(parameterGroup.Value));
            }

            return @params;
        }

        static string[] GetParameterNames(IEnumerable<PropertyInfo> properties) => properties.Select(x => x.GetCustomAttribute<ParameterAttribute>().Name).ToArray();
        
        static bool IsEmptyOrWhitespace(string value) => value != null && string.IsNullOrWhiteSpace(value);

        static bool IsNullableValueType(Type type)
        {
            return type.IsValueType && type.IsGenericType && type.GenericTypeArguments.Length == 1;
        }

        static bool IsNullablePrimitiveType(Type type)
        {
            return IsNullableValueType(type) && type.GenericTypeArguments.Single().IsPrimitive;
        }

        public static void ReadParameter<T>(PropertyInfo property, IDictionary<string, string[]> arguments, T paramsInstances)
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
                && !(propertyType == typeof(string) || propertyType.IsArray))
                throw new NotSupportedException($"{nameof(AllowEmptyAttribute)} is only applicable to String and Optional Array type parameters. Property: {property.Name}");

            if (isOptional && optionalAttr.DefaultValue != null
                && propertyType.IsArray)
                throw new NotSupportedException($"Optional Array type parameters can't have default values - there's no good reason for that. Property: {property.Name}");

            var eitherOrAttr = property.GetCustomAttribute<EitherOrAttribute>(false);
            bool isEitherOr = eitherOrAttr != null;

            if (isOptional && isEitherOr)
                throw new InvalidOperationException($"{nameof(OptionalAttribute)} and {nameof(EitherOrAttribute)} are incompatible at the moment");

            if (propertyType.IsValueType)
            {
                HandleValueType(arguments, property, paramAttr, optionalAttr, paramsInstances);
            }
            else
            {
                if (propertyType == typeof(string))
                {
                    string[] values;
                    var argExists = arguments.TryGetValue(paramAttr.Name, out values);

                    if (!argExists)
                    {
                        if (isOptional)
                        {
                            if (optionalAttr.DefaultValue != null)
                                property.SetValue(paramsInstances, optionalAttr.DefaultValue);
                            return;
                        }
                        else if (isEitherOr)
                            return;
                        else
                            throw new ParameterMissingException(paramAttr.Name);
                    }
                    else
                    {
                        string value = values.Last();

                        if (propertyType == typeof(string))
                        {
                            if ((value == null && !isEitherOr) || (IsEmptyOrWhitespace(value) && !isEmptyAllowed))
                                throw new ParameterEmptyException(paramAttr.Name);
                            else
                                property.SetValue(paramsInstances, value?.Trim());
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

                    string[] values;
                    var argExists = arguments.TryGetValue(paramAttr.Name, out values);

                    if (!argExists)
                    {
                        if (isOptional || isEitherOr)
                            return;
                        else
                            throw new ParameterMissingException(paramAttr.Name);
                    }
                    else
                    {
                        if (values.Length == 1)
                        {
                            string value = values.First();
                            if (value == null)
                            {
                                if (isEitherOr)
                                    return;
                                else
                                throw new ParameterEmptyException(paramAttr.Name);
                            }
                            else if (string.IsNullOrWhiteSpace(value))
                            {
                                if (!isEmptyAllowed)
                                {
                                    if (isEitherOr)
                                        return;
                                    else
                                    throw new ParameterEmptyException(paramAttr.Name);
                                }
                                else
                                {
                                    property.SetValue(paramsInstances, Array.Empty<string>());
                                    return;
                                }
                            }
                            else
                            {
                                var split = value.Split(ArrayValueSeparator);
                                if (split.Any(string.IsNullOrWhiteSpace))
                                    throw new BadParameterValueException(paramAttr.Name, "An array is not allowed to contain empty elements");

                                var arrValue = split.ToArray();

                                property.SetValue(paramsInstances, arrValue);
                                return;
                            }
                        }
                        // More than one array item - provided as separate args, not separator-separated string
                        else
                        {
                            if (values.Any(string.IsNullOrWhiteSpace))
                                throw new BadParameterValueException(paramAttr.Name, "An array is not allowed to contain empty elements");
                            else
                            {
                                property.SetValue(paramsInstances, values);
                                return;
                            }
                        }
                    }
                }
                else
                    throw new NotSupportedException($"Not supported type: {property.Name} ({propertyType.FullName})");
            }
        }

        static void HandleValueType<TTarget>(IDictionary<string, string[]> arguments, PropertyInfo property, ParameterAttribute paramAttr, OptionalAttribute optionalAttr, TTarget paramsInstances)
        {
            var propertyType = property.PropertyType;
            var isOptional = optionalAttr != null;

            if (propertyType.IsPrimitive || IsNullablePrimitiveType(propertyType))
            {
                string[] values;
                var argExists = arguments.TryGetValue(paramAttr.Name, out values);

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
                    string value = values.Last();

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
    }
}