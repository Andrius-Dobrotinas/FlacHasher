using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Hashfile.Read
{
    public interface IHashEntryCollectionParser
    {
        IEnumerable<KeyValuePair<string, string>> Parse(IEnumerable<string> lines);
    }

    /// <summary>
    /// If <see cref="lineParser"/> returns null, skips the line.
    /// In case of exception, rethrows it with the number of the line.
    /// </summary>
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
                var result = ParseLine(line, ++i);
                if (result == null) continue;
                yield return result.Value;
            }
        }

        private KeyValuePair<string, string>? ParseLine(string line, int lineNumber)
        {
            try
            {
                return lineParser.Parse(line);
            }
            catch (Exception e)
            {
                throw new HashFileException($"Error parsing hash file's line #{lineNumber}: {e.Message}", e);
            }
        }
    }
}