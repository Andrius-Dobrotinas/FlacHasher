using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public interface IValidatingHashMapParser
    {
        IEnumerable<KeyValuePair<string, string>> Parse(IEnumerable<string> lines);
    }

    public class ValidatingHashMapParser : IValidatingHashMapParser
    {
        private readonly IHashEntryCollectionParser collectionParser;
        private readonly IEqualityComparer<string> stringComparer;

        public ValidatingHashMapParser(IHashEntryCollectionParser collectionParser, IEqualityComparer<string> stringComparer)
        {
            this.collectionParser = collectionParser;
            this.stringComparer = stringComparer;
        }

        /// <summary>
        /// Uses a default string Comparer for file names
        /// </summary>
        public ValidatingHashMapParser(IHashEntryCollectionParser collectionParser) : this(collectionParser, null)
        {
        }

        public IEnumerable<KeyValuePair<string, string>> Parse(IEnumerable<string> lines)
        {
            var processedFilenames = new List<string>();
            bool firstItemHasFileName = false;
            int index = 0;

            foreach (var entry in collectionParser.Parse(lines))
            {
                yield return Validate(entry, processedFilenames, index++, ref firstItemHasFileName);
            }
        }

        private KeyValuePair<string, string> Validate(KeyValuePair<string, string> entry, IList<string> processedFilenames, int index, ref bool firstItemHasFileName)
        {
            if (index == 0)
                firstItemHasFileName = entry.Key != null;

            if (index > 0)
            {
                var hasFilename = entry.Key != null;

                if ((hasFilename && !firstItemHasFileName)
                    || (!hasFilename && firstItemHasFileName))
                    throw new MissingFileNameException();
            }

            if (firstItemHasFileName)
            {
                if (processedFilenames.Contains(entry.Key, stringComparer))
                    throw new DuplicateFileException(entry.Key, index + 1);

                processedFilenames.Add(entry.Key);
            }

            if (entry.Value == null)
                throw new MissingHashValueException(index + 1);

            return entry;
        }
    }
}