using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Hashfile.Read
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

        public static class Default
        {
            public const string HashfileEntrySeparator = ":";

            public static HashFileReader BuildHashfileReader(string hashfileEntrySeparator, bool ignoreNonhashLines)
            {
                return new HashFileReader(
                            new HashMapParser(
                                new HashEntryCollectionParser(
                                    hashfileEntrySeparator == null
                                        ? new HashEntryParserAdvanced()
                                        : new HashEntryParser(hashfileEntrySeparator),
                                    ignoreNonhashLines),
                                new CaseInsensitiveOrdinalStringComparer()));
            }

            public static HashFileReader BuildAdvancedHashfileReader(bool ignoreNonhashLines)
            {
                return BuildHashfileReader(null, ignoreNonhashLines);
            }
        }
    }
}