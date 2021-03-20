using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class HashFileReader
    {
        private readonly IHashMapParser parser;

        public HashFileReader(IHashMapParser parser)
        {
            this.parser = parser;
        }

        public FileHashMap Read(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName)
                .Where(line => string.IsNullOrWhiteSpace(line) == false);

            var expectedHashes = parser
                .Parse(lines);

            return expectedHashes;
        }
    }
}