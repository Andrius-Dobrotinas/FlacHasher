using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Verification
{
    public interface IHashEntryParser
    {
        KeyValuePair<string, string> Parse(string line);
    }

    public class HashEntryParser : IHashEntryParser
    {
        public KeyValuePair<string, string> Parse(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (string.IsNullOrWhiteSpace(line)) throw new ArgumentException("An empty string is unacceptable!", nameof(line));

            // Returns 1 segment if there's no separator
            var segments = line.Split(':');

            if (segments.Length == 1)
                return new KeyValuePair<string, string>(
                    null,
                    TrimAndReplaceEmptyWithNull(segments[0]));

            if (segments.Length > 2)
                throw new Exception($"Expected the line to have 1-2 segments, but it has {segments.Length}");

            return new KeyValuePair<string, string>(
                    TrimAndReplaceEmptyWithNull(segments[0]),
                    TrimAndReplaceEmptyWithNull(segments[1]));
        }

        private string TrimAndReplaceEmptyWithNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            else
                return value.Trim();
        }
    }
}