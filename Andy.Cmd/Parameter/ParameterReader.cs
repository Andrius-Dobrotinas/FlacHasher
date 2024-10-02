﻿using System;
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
        public static TParam GetParameters<TParam>(IDictionary<string, string[]> arguments, bool inLowercase = false)
            where TParam : new()
        {
            var properties = typeof(TParam).GetProperties();

            EnsureParameterNameUniqueness(properties, inLowercase);

            // Validate attribute use before reading the values because this is configured before compile-time
            var eitherOrPropertyGroups = GetEitherOrPropertyGroups<TParam>();

            var @params = new TParam();
            foreach (var property in properties)
            {
                ReadParameter(property, arguments, @params, inLowercase);
            }

            CheckConditionallyRequiredOnes(@params, properties);

            // Either-Or Continued
            CheckEitherOrParameters(@params, eitherOrPropertyGroups);

            Check_AtLeastOneOf_Params(@params);

            return @params;
        }

        static void EnsureParameterNameUniqueness(IEnumerable<PropertyInfo> properties, bool inLowercase)
        {
            var paramNames = properties.SelectMany(p => p.GetCustomAttributes<ParameterAttribute>(false))
                .Where(attr => attr != null)
                .Select(attr => inLowercase ? attr.Name.ToLowerInvariant() : attr.Name)
                .ToList();

            if (paramNames.Distinct().Count() != paramNames.Count)
                throw new InvalidOperationException("Some parameter names are repeated");
        }

        /// <summary>
        /// Gathers <see cref="EitherOrAttribute"/> properties, grouped by their <see cref="EitherOrAttribute.GroupKey"/>.
        /// Carries out necessary attribute use validations
        /// </summary>
        public static Dictionary<string, PropertyInfo[]> GetEitherOrPropertyGroups<TParams>()
        {
            var properties = typeof(TParams).GetProperties();

            var eitherOrProperties = properties.Select(property => new { property, attr = property.GetCustomAttribute<EitherOrAttribute>(false) })
                .Where(x => x.attr != null)
                .ToList();

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

            return eitherOrPropertyGroups;
        }

        public static void CheckEitherOrParameters<TParams>(TParams instance, IDictionary<string, PropertyInfo[]> eitherOrPropertyGroups)
        {
            foreach (var parameterGroup in eitherOrPropertyGroups)
            {
                var values = parameterGroup.Value.Select(x => x.GetValue(instance));
                var nullValues = values.Select(x => x == null);
                if (nullValues.Count(x => x == false) > 1)
                    throw new ParameterGroupException("Only one parameter is allowed to have a value", GetParameterNames(parameterGroup.Value));

                if (nullValues.All(x => x == true))
                    throw new ParameterGroupException("One of the following parameter must have a value", GetParameterNames(parameterGroup.Value));
            }
        }

        static Dictionary<string, PropertyInfo[]> Get_AtLeastOneOf_Properties<TParams>()
        {
            var properties = typeof(TParams).GetProperties();

            var propertiesOfInterest = properties.Select(
                property => new 
                    { 
                        property, 
                        attr = property.GetCustomAttributes<AtLeastOneOfAttribute>(false).ToArray()
                    })
                .Where(x => x.attr.Any())
                .SelectMany(
                    x => x.attr.Select(
                        attr => (property: x.property, attr: attr))
                    .ToArray())
                .ToList();

            var propertyOfInterestGroups = propertiesOfInterest
               .GroupBy(x => x.attr.GroupKey, x => x.property)
               .ToDictionary(x => x.Key, x => x.ToArray());

            return propertyOfInterestGroups;
        }

        static void Check_AtLeastOneOf_Params<TParams>(TParams instance)
        {
            var propertyOfInterestGroups = Get_AtLeastOneOf_Properties<TParams>();
            foreach (var parameterGroup in propertyOfInterestGroups)
            {
                var values = parameterGroup.Value.Select(x => x.GetValue(instance));
                var nullValues = values.Select(x => x == null);

                if (nullValues.All(x => x == true))
                    throw new ParameterGroupException("At least one of the following parameter must have a value", GetParameterNames(parameterGroup.Value));
            }
        }

        public static void CheckConditionallyRequiredOnes<TParams>(TParams instance, IEnumerable<PropertyInfo> allProperties)
        {
            var propertiesOfInterest = allProperties.Select(x => (property: x, attr: x.GetCustomAttributes<RequiredWithAttribute>(false)))
                .Where(x => x.attr.Any())
                .SelectMany(x => x.attr.Select(attr => (property: x.property, attr: attr)).ToArray())
                .GroupBy(x => x.attr.OtherPropertyName);

            foreach (var dependencyGroup in propertiesOfInterest)
            {
                var targetProperty = allProperties.FirstOrDefault(x => x.Name == dependencyGroup.Key);
                if (targetProperty == null)
                    throw new InvalidOperationException($"{nameof(RequiredWithAttribute)} master property doesn't exist: {dependencyGroup.Key}");

                var value = targetProperty.GetValue(instance);
                var isBool = targetProperty.PropertyType == typeof(bool) || targetProperty.PropertyType == typeof(bool?);
                if ((value != null && !isBool) || (isBool && (value != null && (bool)value == true)))
                    foreach (var p in dependencyGroup)
                    {
                        var hasValue = p.property.GetValue(instance) != null;
                        if (!hasValue)
                        {
                            var eitherOrAttr = p.property.GetCustomAttribute<EitherOrAttribute>(false);
                            if (eitherOrAttr == null)
                                throw new ParameterDependencyUnmetException(p.property, dependencyGroup.Key);

                            // either-or check will get this
                        }
                    }
            }
        }

        static string[] GetParameterNames(IEnumerable<PropertyInfo> properties) => properties
            .SelectMany(
                x => x.GetCustomAttributes<ParameterAttribute>(false).Select(x => x.Name))
            .ToArray();

        static bool IsEmptyOrWhitespace(string value) => value != null && string.IsNullOrWhiteSpace(value);

        static bool IsNullableValueType(Type type)
        {
            return type.IsValueType && type.IsGenericType && type.GenericTypeArguments.Length == 1;
        }

        static bool IsNullablePrimitiveType(Type type)
        {
            return IsNullableValueType(type) && type.GenericTypeArguments.Single().IsPrimitive;
        }

        public static void ReadParameter<T>(PropertyInfo property, IDictionary<string, string[]> arguments, T paramsInstances, bool inLowercase = false)
        {
            var paramAttrs = property.GetCustomAttributes<ParameterAttribute>(false);
            if (!paramAttrs.Any())
                return;

            var propertyType = property.PropertyType;
            if (!(propertyType == typeof(string)
                || propertyType.IsPrimitive
                || IsNullablePrimitiveType(propertyType)
                || (propertyType.IsArray && propertyType.HasElementType)))
                throw new NotSupportedException($"Only primitive value types, strings and arrays strings have been implemented. Property: {property.Name}");

            // Actual Optional attr should be prioritized because RequiredWith and EitherOr don't specify default values; only take them if actual Optional is not there.
            var optionalAttrs = property.GetCustomAttributes<OptionalAttribute>(false);
            var isOptional = optionalAttrs.Any();
            if (isOptional
                && optionalAttrs.Any(x => (x is RequiredWithAttribute) || (x is EitherOrAttribute) || (x is AtLeastOneOfAttribute))
                && optionalAttrs.Any(x => x.DefaultValue != null))
                throw new InvalidOperationException($"A parameter marked with {nameof(RequiredWithAttribute)}, {nameof(EitherOrAttribute)} or {nameof(AtLeastOneOfAttribute)} is not allowed to have a default value");
            var optionalAttr = optionalAttrs.FirstOrDefault();

            var isEmptyAllowed = property.GetCustomAttributes(typeof(AllowEmptyAttribute), false).SingleOrDefault() as AllowEmptyAttribute != null;

            if (isEmptyAllowed
                && !(propertyType == typeof(string) || propertyType.IsArray))
                throw new NotSupportedException($"{nameof(AllowEmptyAttribute)} is only applicable to String and Optional Array type parameters. Property: {property.Name}");

            if (isOptional && optionalAttr.DefaultValue != null
                && propertyType.IsArray)
                throw new NotSupportedException($"Optional Array type parameters can't have default values - there's no good reason for that. Property: {property.Name}");

            var paramNamesPrioritized = paramAttrs.OrderBy(x => x.Order).Select(x => x.Name);
            if (inLowercase)
                paramNamesPrioritized = paramNamesPrioritized.Select(x => x.ToLowerInvariant());

            if (propertyType.IsValueType)
            {
                HandleValueType(arguments, property, paramNamesPrioritized, optionalAttr, paramsInstances);
            }
            else
            {
                if (propertyType == typeof(string))
                {
                    HandleStringParam(arguments, property, paramNamesPrioritized, optionalAttr, isEmptyAllowed, paramsInstances);
                }
                else if (propertyType.IsArray)
                {
                    if (!propertyType.HasElementType)
                        throw new NotSupportedException($"Array type without its element type: {property.Name} ({propertyType.FullName})");

                    var elementType = propertyType.GetElementType();
                    if (elementType != typeof(string))
                        throw new NotSupportedException($"Array of other than Strings is not supported: {property.Name}, {propertyType.FullName}");

                    HandleArrayParam(arguments, property, paramNamesPrioritized, isOptional, isEmptyAllowed, paramsInstances);
                }
                else
                    throw new NotSupportedException($"Not supported type: {property.Name} ({propertyType.FullName})");
            }
        }

        static void SetArrayValueTrimmed<TParams>(TParams paramsInstances, PropertyInfo property, string paramName, string[] values)
        {
            var valuesTrimmed = TrimValues(values);
            if (valuesTrimmed.Any(string.IsNullOrEmpty))
                throw new BadParameterValueException(paramName, "An array is not allowed to contain empty elements");

            property.SetValue(paramsInstances, valuesTrimmed);
        }

        static string[] TrimValues(IEnumerable<string> values) => values.Select(x => x?.Trim()).ToArray();

        static bool TryGetFirstPresentValue(IDictionary<string, string[]> arguments, IEnumerable<string> paramNames, out string foundParamName, out string[] values)
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

        static void HandleStringParam<TTarget>(IDictionary<string, string[]> arguments, PropertyInfo property, IEnumerable<string> paramNamesPrioritized, OptionalAttribute optionalAttr, bool isEmptyAllowed, TTarget paramsInstances)
        {
            if (!paramNamesPrioritized.Any())
                throw new ArgumentOutOfRangeException(nameof(paramNamesPrioritized), "At least one parameter name must be provided");

            var isOptional = optionalAttr != null;

            string paramName;
            string[] values;
            bool argExists = TryGetFirstPresentValue(arguments, paramNamesPrioritized, out paramName, out values);

            if (!argExists)
            {
                if (isOptional)
                {
                    if (optionalAttr.DefaultValue != null)
                        property.SetValue(paramsInstances, optionalAttr.DefaultValue);
                    return;
                }
                else
                    throw new ParameterMissingException(property);
            }
            else
            {
                string value = values.Last();

                if ((value == null) || (IsEmptyOrWhitespace(value) && !isEmptyAllowed))
                    throw new ParameterEmptyException(paramName);
                else
                    property.SetValue(paramsInstances, value.Trim());
            }
        }

        static void HandleArrayParam<TTarget>(IDictionary<string, string[]> arguments, PropertyInfo property, IEnumerable<string> paramNamesPrioritized, bool isOptional, bool isEmptyAllowed, TTarget paramsInstances)
        {
            if (!paramNamesPrioritized.Any())
                throw new ArgumentOutOfRangeException(nameof(paramNamesPrioritized), "At least one parameter name must be provided");

            string paramName;
            string[] values;
            bool argExists = TryGetFirstPresentValue(arguments, paramNamesPrioritized, out paramName, out values);

            if (!argExists)
            {
                if (isOptional)
                    return;
                else
                    throw new ParameterMissingException(property);
            }
            else
            {
                if (values.Length == 1)
                {
                    string value = values.First();
                    if (value == null)
                    {
                        throw new ParameterEmptyException(paramName);
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        if (!isEmptyAllowed)
                        {
                            throw new ParameterEmptyException(paramName);
                        }
                        else
                        {
                            // Empty string for an array means an empty array
                            property.SetValue(paramsInstances, Array.Empty<string>());
                        }
                    }
                    else
                    {
                        var split = value.Split(ArrayValueSeparator);
                        SetArrayValueTrimmed(paramsInstances, property, paramName, split);
                    }
                }
                // More than one array item - provided as separate args, not separator-separated string
                else
                {
                    SetArrayValueTrimmed(paramsInstances, property, paramName, values);
                }
            }
        }

        static void HandleValueType<TTarget>(IDictionary<string, string[]> arguments, PropertyInfo property, IEnumerable<string> paramNamesPrioritized, OptionalAttribute optionalAttr, TTarget paramsInstances)
        {
            if (!paramNamesPrioritized.Any())
                throw new ArgumentOutOfRangeException(nameof(paramNamesPrioritized), "At least one parameter name must be provided");

            var propertyType = property.PropertyType;
            var isOptional = optionalAttr != null;

            if (propertyType.IsPrimitive || IsNullablePrimitiveType(propertyType))
            {
                string paramName;
                string[] values;
                bool argExists = TryGetFirstPresentValue(arguments, paramNamesPrioritized, out paramName, out values);

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
                        throw new ParameterMissingException(property);
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
                            throw new ParameterEmptyException(paramName);

                        object parsedValue;
                        try
                        {
                            parsedValue = ParseNonNullPrimitive(value, property.PropertyType);
                        }
                        catch (FormatException e)
                        {
                            throw new BadParameterValueException(paramName, e);
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
    }
}