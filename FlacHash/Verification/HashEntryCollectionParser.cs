using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public interface IHashEntryCollectionParser
    {
        IEnumerable<KeyValuePair<string, string>> Parse(IEnumerable<string> lines);
    }

    public class HashEntryCollectionParser : IHashEntryCollectionParser
    {
        private readonly IHashEntryParser lineParser;

        public HashEntryCollectionParser(IHashEntryParser lineParser)
        {
            this.lineParser = lineParser;
        }

        public IEnumerable<KeyValuePair<string, string>> Parse(IEnumerable<string> lines)
        {
            int i = 0;
            return lines.Select(line => lineParser.Parse(line, ++i));
        }
    }
}