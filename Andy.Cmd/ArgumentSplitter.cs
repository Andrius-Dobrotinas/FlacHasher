using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd
{
    public static class ArgumentSplitter
    {
        /// <summary>
        /// When a parameter is provided without an equals sign, its value is null
        /// When a parameter is provided with an equals sign, its value is an empty string
        /// </summary>
        public static IDictionary<string, string[]> GetArguments(string[] args)
        {
            var argParser = new ArgumentParser('=');

            return args.Select(argParser.Parse)
                .GroupBy(x => x.Key, x=> x.Value)
                .ToDictionary(x => x.Key, x => x.ToArray());
        }
    }
}