using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd
{
    public static class ArgumentSplitter
    {
        /// <summary>
        /// Groups parameters by name so arrays can be supplied as discrete elements
        /// When a parameter is provided without an equals sign, its value is null.
        /// When a parameter is provided with an equals sign but no value, its value is an empty string.
        /// </summary>
        public static IDictionary<string, string[]> GetArguments(string[] args, bool paramNamesToLowercase = false)
        {
            var argParser = new ArgumentParser('=', paramNamesToLowercase);

            return args.Select(argParser.Parse)
                .GroupBy(x => x.Key, x=> x.Value)
                .ToDictionary(
                    x => x.Key,
                    x =>
                    {
                        var values = x.ToArray();
                        var flagCount = values.Count(x => x == null);
                        var nonFlagCount = values.Count(x => x != null);
                        if (nonFlagCount > 0 && flagCount > 0)
                            throw new ArgumentException($"A parameter can't be supplied as a mix of key=value pairs and flags", x.Key);
                        else if (flagCount > 1)
                            throw new ArgumentException($"A flag parameter can't be supplied more than once", x.Key);
                        return values;
                    });
        }
    }
}