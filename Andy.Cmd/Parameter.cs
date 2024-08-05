using System;
using System.Collections.Generic;

namespace Andy.Cmd
{
    public static class Parameter
    {
        public static bool? GetBoolValue(IDictionary<string, string> arguments, string argName)
        {
            if (!arguments.ContainsKey(argName))
                return null;

            string value = TryGetValueAllowingEmpty(arguments, argName);

            return String.IsNullOrEmpty(value)
                ? true
                : bool.Parse(value);
        }

        public static T GetValueOptional<T>(IDictionary<string, string> arguments, string argName, Func<string, T> createAnInstance)
        {
            string value = GetValueOptional(arguments, argName);

            return value == null
                ? default(T)
                : createAnInstance(value);
        }

        /// <summary>
        /// If the argument is present, returns its value regardless whether it actually has one (eg empty string).
        /// If it happens not to have a value, returns <paramref name="valueIfEmpty"/> value or an empty string.
        /// If the argument is not present, returns null.
        /// </summary>
        public static string TryGetValueAllowingEmpty(IDictionary<string, string> arguments, string argName, string valueIfEmpty = null)
        {
            string value;

            if (!arguments.TryGetValue(argName, out value))
                return null;

            return string.IsNullOrEmpty(value)
                ? valueIfEmpty ?? ""
                : value;
        }

        /// <summary>
        /// If the argument is present, returns the value or throws an exception if it's empty.
        /// If the argument is not found, returns null.
        /// </summary>
        public static string GetValueOptional(IDictionary<string, string> arguments, string argName)
        {
            string value;

            return arguments.TryGetValue(argName, out value)
                ? string.IsNullOrWhiteSpace(value)
                    ? throw new ParameterException("Parameter supplied without value", argName)
                    : value
                : null;
        }
    }
}