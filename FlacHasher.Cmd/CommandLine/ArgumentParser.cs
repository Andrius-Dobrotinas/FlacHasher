using System;
using System.Collections.Generic;

namespace FlacHasher.CommandLine
{
    public class ArgumentParser
    {
        public KeyValuePair<string, string> ParseArgument(string argument)
        {
            var separatorIndex = argument.IndexOf('=');
            if (separatorIndex == -1)
            {
                return new KeyValuePair<string, string>(argument, null);
            }

            var name = argument.Substring(0, separatorIndex);
            var value = argument.Substring(separatorIndex + 1);

            return new KeyValuePair<string, string>(name, value);
        }
    }
}