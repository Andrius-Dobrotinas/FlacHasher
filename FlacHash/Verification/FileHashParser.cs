using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public interface IFileHashParser
    {
        IEnumerable<KeyValuePair<string, string>> Parse(IEnumerable<string> lines);
    }

    public class FileHashParser : IFileHashParser
    {
        private readonly IHashEntryParser lineParser;

        public FileHashParser(IHashEntryParser lineParser)
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