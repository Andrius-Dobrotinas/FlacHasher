using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Hashing.Verification
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

            foreach (var line in lines)
            {
                yield return ParseLine(line, ++i);
            }
        }

        private KeyValuePair<string, string> ParseLine(string line, int lineNumber)
        {
            try
            {
                return lineParser.Parse(line);
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing hash file's line #{lineNumber}: {e.Message}", e);
            }
        }
    }
}