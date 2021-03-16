using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public interface IHashMapParser
    {
        IEnumerable<KeyValuePair<string, string>> Parse(IEnumerable<string> lines);
    }

    public class HashMapParser : IHashMapParser
    {
        private readonly IHashEntryParser lineParser;

        public HashMapParser(IHashEntryParser lineParser)
        {
            this.lineParser = lineParser;
        }

        public IEnumerable<KeyValuePair<string, string>> Parse(IEnumerable<string> lines)
        {
            int i = 0;
            var parsedLines = lines.Select(line => lineParser.ParseLine(line, ++i));

            return parsedLines.Select(
                segments => new KeyValuePair<string, string>(
                    segments[0],
                    segments[1]));
        }
    }
}