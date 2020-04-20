using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Configuration.Ini
{
    public interface IEntryParser
    {
        KeyValuePair<string, string>? Parse(string line);
    }

    public class EntryParser : IEntryParser
    {
        public KeyValuePair<string, string>? Parse(string line)
        {
            var separatorIndex = line.IndexOf('=');

            if (separatorIndex > 0)
            {
                var key = line.Substring(0, separatorIndex);
                var value = line.Substring(separatorIndex + 1);
                return new KeyValuePair<string, string>(key.Trim(), value);
            }
            
            return null;
        }
    }
}