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
        private readonly string separator;

        /// <param name="separator">Any separator goes, except for empty value and new-line characters</param>
        public HashEntryParser(string separator)
        {
            if (string.IsNullOrEmpty(separator))
                throw new ArgumentNullException(nameof(separator), "A separator cannot be an empty value");

            if (separator == "\r" || separator == "\n" || separator == "\r\n")
                throw new ArgumentException(nameof(separator), "A separator cannot be a New-line value");

            this.separator = separator;
        }

        /// <summary>
        /// If no segment separator char/sequence is found in the <paramref name="line"/>, it returns the whole line as Value and null for Key.
        /// If a segment separator char/sequence is present in the <paramref name="line"/>, it splits the the line into two segments
        /// and returns the 1st segment as Key and the 2nd one as as Value.
        /// If the separator char/sequence is found more than once, it treats the last instance as a separator --
        /// therefore, it's safe for the first segment to have a separator char/sequence as a legitimate part of its value.
        /// </summary>
        public KeyValuePair<string, string> Parse(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (string.IsNullOrWhiteSpace(line)) throw new ArgumentException("An empty string is unacceptable!", nameof(line));

            var separatorIndex = line.LastIndexOf(separator);
            
            if (separatorIndex == -1)
                // Return the whole line as value
                return new KeyValuePair<string, string>(
                    null,
                    TrimAndReplaceEmptyWithNull(line));

            var segment1 = line.Substring(0, separatorIndex);
            var segment2 = line.Substring(separatorIndex + separator.Length);

            return new KeyValuePair<string, string>(
                    TrimAndReplaceEmptyWithNull(segment1),
                    TrimAndReplaceEmptyWithNull(segment2));
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