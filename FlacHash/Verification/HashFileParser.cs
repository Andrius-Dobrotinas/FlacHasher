using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class HashFileParser
    {
        private readonly IHashEntryParser lineParser;

        public HashFileParser(IHashEntryParser lineParser)
        {
            this.lineParser = lineParser;
        }

        public IEnumerable<KeyValuePair<string, string>> Parse(string[] lines)
        {
            int i = 0;
            var parsedLines = lines.Select(line => lineParser.ParseLine(line, i++));

            return parsedLines.Select(
                segments => new KeyValuePair<string, string>(
                    segments[0],
                    segments[1]));
        }
    }
}