using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class HashFileParser
    {
        private readonly IValidatingFileHashParser parser;

        public HashFileParser(IValidatingFileHashParser parser)
        {
            this.parser = parser;
        }

        public IEnumerable<KeyValuePair<string, string>> Parse(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName)
                .Where(line => string.IsNullOrWhiteSpace(line) == false);

            var expectedHashes = parser
                .Parse(lines);

            return expectedHashes;
        }
    }
}