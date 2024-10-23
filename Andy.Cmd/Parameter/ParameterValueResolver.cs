using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public interface IParameterValueResolver
    {
        void ReadParameter<T>(PropertyInfo property, IDictionary<string, string[]> arguments, T paramsInstances, bool inLowercase = false);
    }

    public class ParameterValueResolver : IParameterValueResolver
    {
        public const char ArrayValueSeparator = ';';

        public void ReadParameter<T>(PropertyInfo property, IDictionary<string, string[]> arguments, T paramsInstances, bool inLowercase = false)
        {
            var paramAttrs = property.GetCustomAttributes<ParameterAttribute>(false);
            if (!paramAttrs.Any())
                return;

            var propertyType = property.PropertyType;
            if (!(propertyType == typeof(string)
                || propertyType.IsPrimitive
                || propertyType.IsEnum
                || ParameterValueResolverFunctions.IsNullablePrimitiveOrEnumType(propertyType)
                || (propertyType.IsArray && propertyType.HasElementType)))
                throw new NotSupportedException($"Only primitive value types, strings and arrays strings have been implemented. Property: {property.Name}");

            // Actual Optional attr should be prioritized because RequiredWith and EitherOr don't specify default values; only take them if actual Optional is not there.
            var optionalAttrs = property.GetCustomAttributes<OptionalAttribute>(false);
            var isOptional = optionalAttrs.Any();
            if (isOptional
                && optionalAttrs.Any(x => (x is RequiredWithAttribute) || (x is EitherOrAttribute))
                && optionalAttrs.Any(x => x.DefaultValue != null))
                throw new InvalidOperationException($"A parameter marked with {nameof(RequiredWithAttribute)} or {nameof(EitherOrAttribute)} is not allowed to have a default value");

            // More than one is allowed, by only one non-derivative Optional is allowed.
            // One with default value is preferred (just for the said default value)
            var optionalAttr = optionalAttrs.FirstOrDefault(x => x.DefaultValue != null) ?? optionalAttrs.FirstOrDefault();

            var isEmptyAllowed = property.GetCustomAttributes(typeof(AllowEmptyAttribute), false).SingleOrDefault() as AllowEmptyAttribute != null;

            if (isEmptyAllowed
                && !(propertyType == typeof(string) || propertyType.IsArray))
                throw new NotSupportedException($"{nameof(AllowEmptyAttribute)} is only applicable to String and Optional Array type parameters. Property: {property.Name}");

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

                    HandleArrayParam(arguments, property, paramNamesPrioritized, optionalAttr, isEmptyAllowed, paramsInstances);
                }
                else
                    throw new NotSupportedException($"Not supported type: {property.Name} ({propertyType.FullName})");
            }
        }

        static void HandleStringParam<TTarget>(IDictionary<string, string[]> arguments, PropertyInfo property, IEnumerable<string> paramNamesPrioritized, OptionalAttribute optionalAttr, bool isEmptyAllowed, TTarget paramsInstances)
        {
            if (!paramNamesPrioritized.Any())
                throw new ArgumentOutOfRangeException(nameof(paramNamesPrioritized), "At least one parameter name must be provided");

            var isOptional = optionalAttr != null;

            string paramName;
            string[] values;
            bool argExists = ParameterValueResolverFunctions.TryGetFirstPresentValue(arguments, paramNamesPrioritized, out paramName, out values);

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

                if ((value == null) || (ParameterValueResolverFunctions.IsEmptyOrWhitespace(value) && !isEmptyAllowed))
                    throw new ParameterEmptyException(paramName);
                else
                    property.SetValue(paramsInstances, value.Trim());
            }
        }

        static void HandleArrayParam<TTarget>(IDictionary<string, string[]> arguments, PropertyInfo property, IEnumerable<string> paramNamesPrioritized, OptionalAttribute optionalAttr, bool isEmptyAllowed, TTarget paramsInstances)
        {
            if (!paramNamesPrioritized.Any())
                throw new ArgumentOutOfRangeException(nameof(paramNamesPrioritized), "At least one parameter name must be provided");

            string paramName;
            string[] values;
            bool argExists = ParameterValueResolverFunctions.TryGetFirstPresentValue(arguments, paramNamesPrioritized, out paramName, out values);

            if (!argExists)
            {
                var isOptional = optionalAttr != null;
                if (isOptional)
                {
                    if (optionalAttr.DefaultValue != null)
                    {
                        var split = optionalAttr.DefaultValue is string[]
                            ? (string[])optionalAttr.DefaultValue
                            : ((string)optionalAttr.DefaultValue).Split(ArrayValueSeparator);
                        ParameterValueResolverFunctions.SetArrayValueTrimmed(paramsInstances, property, paramName, split);
                    }
                    return;
                }
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
                        ParameterValueResolverFunctions.SetArrayValueTrimmed(paramsInstances, property, paramName, split);
                    }
                }
                // More than one array item - provided as separate args, not separator-separated string
                else
                {
                    ParameterValueResolverFunctions.SetArrayValueTrimmed(paramsInstances, property, paramName, values);
                }
            }
        }

        static void HandleValueType<TTarget>(IDictionary<string, string[]> arguments, PropertyInfo property, IEnumerable<string> paramNamesPrioritized, OptionalAttribute optionalAttr, TTarget paramsInstances)
        {
            if (!paramNamesPrioritized.Any())
                throw new ArgumentOutOfRangeException(nameof(paramNamesPrioritized), "At least one parameter name must be provided");

            var propertyType = property.PropertyType;
            var isOptional = optionalAttr != null;

            if (propertyType.IsPrimitive || propertyType.IsEnum || ParameterValueResolverFunctions.IsNullablePrimitiveOrEnumType(propertyType))
            {
                string paramName;
                string[] values;
                bool argExists = ParameterValueResolverFunctions.TryGetFirstPresentValue(arguments, paramNamesPrioritized, out paramName, out values);

                if (!argExists)
                    if (isOptional)
                    {
                        if (optionalAttr.DefaultValue != null)
                            property.SetValue(paramsInstances, optionalAttr.DefaultValue);
                        return;
                    }
                    else if (ParameterValueResolverFunctions.IsNullablePrimitiveOrEnumType(propertyType))
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
                            parsedValue = ParameterValueResolverFunctions.ParseNonNullPrimitive(value, property.PropertyType);
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
    }
}
