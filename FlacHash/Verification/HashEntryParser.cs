using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Verification
{
    public interface IHashEntryParser
    {
        string[] ParseLine(string line, int lineNumber);
    }

    public class HashEntryParser : IHashEntryParser
    {
        public string[] ParseLine(string line, int lineNumber)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (line == "") throw new ArgumentException("An empty string is unacceptable!", nameof(line));

            // Returns 1 segment if there's no separator
            var segments = line.Split(':');

            if (segments.Length == 1)
                return new string[]
                {
                    null, segments[0]
                };

            if (segments.Length > 2)
                throw new Exception($"Expected line {lineNumber} to have {2} segments, but it has {segments.Length}");

            for (int i = 0; i < 2; i++)
                if (segments[i] == "")
                    segments[i] = null;

            return segments;
        }
    }
}