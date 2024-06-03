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

            string value = GetValue(arguments, argName);

            return String.IsNullOrEmpty(value)
                ? true
                : bool.Parse(value);
        }

        public static T GetValue<T>(IDictionary<string, string> arguments, string argName, Func<string, T> createAnInstance)
        {
            string value = GetValue(arguments, argName);

            return String.IsNullOrEmpty(value)
                ? default(T)
                : createAnInstance(value);
        }

        public static string GetValue(IDictionary<string, string> arguments, string argName)
        {
            string value;

            return arguments.TryGetValue(argName, out value)
                ? value ?? ""
                : null;
        }
    }
}