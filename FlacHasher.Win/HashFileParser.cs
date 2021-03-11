using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Win
{
    public class HashFileParser
    {
        public IEnumerable<KeyValuePair<string, string>> Parse(string[] lines)
        {
            int i = 0;
            var parsedLines = lines.Select(line => ParseLine(line, i++));

            return parsedLines.Select(
                segments => new KeyValuePair<string, string>(
                    segments[0],
                    segments[1]));
        }

        private string[] ParseLine(string line, int lineNumber)
        {
            var segments = line.Split(':');

            if (segments.Length != 2)
                throw new Exception($"Expected line {lineNumber} to have {2} segments, but it has {segments.Length}");

            return segments;
        }
    }
}