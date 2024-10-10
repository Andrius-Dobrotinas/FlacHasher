using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public class ParameterReader
    {
        private static Type[] allowedTypes = new Type[] { typeof(string), typeof(string[]) };

        private readonly IParameterValueResolver parameterValueResolver;

        public ParameterReader(IParameterValueResolver parameterValueResolver)
        {
            this.parameterValueResolver = parameterValueResolver;
        }

        public static ParameterReader Build()
        {
            return new ParameterReader(new ParameterValueResolver());
        }

        /// <summary>
        /// String parameters have null values only if they're not specified; otherwise, it's at least an empty string (if allowed).
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public TParam GetParameters<TParam>(IDictionary<string, string[]> arguments, bool inLowercase = false)
            where TParam : new()
        {
            var properties = typeof(TParam).GetProperties();

            EnsureParameterNameUniqueness(properties, inLowercase);

            // Validate attribute use before reading the values because this is configured before compile-time
            var eitherOrPropertyGroups = GetEitherOrPropertyGroups<TParam>();

            var @params = new TParam();
            foreach (var property in properties)
            {
                parameterValueResolver.ReadParameter(property, arguments, @params, inLowercase);
            }

            CheckConditionallyRequiredOnes(@params, properties);

            // Either-Or Continued
            Check_EitherOr_Parameters(@params, eitherOrPropertyGroups);

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
        static Dictionary<string, PropertyInfo[]> GetEitherOrPropertyGroups<TParams>()
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

        static void Check_EitherOr_Parameters<TParams>(TParams instance, IDictionary<string, PropertyInfo[]> eitherOrPropertyGroups)
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
    }
}