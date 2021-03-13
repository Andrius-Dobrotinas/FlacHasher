using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class ValidatingHashFileParser
    {
        private readonly IHashFileParser lineParser;

        public ValidatingHashFileParser(IHashFileParser lineParser)
        {
            this.lineParser = lineParser;
        }

        public IEnumerable<KeyValuePair<string, string>> Parse(string[] lines)
        {
            var processedFilenames = new List<string>(lines.Length);

            foreach (var entry in lineParser.Parse(lines))
            {
                yield return Validate(entry, processedFilenames);
            }
        }

        private KeyValuePair<string, string> Validate(KeyValuePair<string, string> entry, IList<string> processedFilenames)
        {
            if (entry.Key == null)
                throw new MissingFileNameException();

            if (processedFilenames.Contains(entry.Key))
                throw new DuplicateFileException();

            if (entry.Value == null)
                throw new MissingHashValueException();

            processedFilenames.Add(entry.Key);

            return entry;
        }
    }
}