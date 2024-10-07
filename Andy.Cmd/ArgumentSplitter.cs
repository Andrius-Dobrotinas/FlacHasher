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
                        var values = x.Where(x => x != null).ToArray();
                        return values.Any() ? values : new string[] { null };
                    });
        }
    }
}