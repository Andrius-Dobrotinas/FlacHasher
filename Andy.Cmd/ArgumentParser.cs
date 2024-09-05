using System;
using System.Collections.Generic;

namespace Andy.Cmd
{
    public class ArgumentParser
    {
        private readonly char separator;
        private readonly bool keyToLowercase;

        public ArgumentParser(char separator, bool keyToLowercase)
        {
            this.separator = separator;
            this.keyToLowercase = keyToLowercase;
        }

        public KeyValuePair<string, string> Parse(string argument)
        {
            var separatorIndex = argument.IndexOf(separator);
            if (separatorIndex == -1)
            {
                return new KeyValuePair<string, string>(
                    keyToLowercase ? argument.ToLowerInvariant() : argument,
                    null);
            }

            var name = argument.Substring(0, separatorIndex);
            var value = argument.Substring(separatorIndex + 1);

            return new KeyValuePair<string, string>(
                keyToLowercase ? name.ToLowerInvariant() : name,
                value);
        }
    }
}