using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd
{
    public static class ArgumentSplitter
    {
        public static IDictionary<string, string> GetArguments(string[] args)
        {
            var argParser = new ArgumentParser('=');

            return args.Select(argParser.Parse)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value);
        }
    }
}