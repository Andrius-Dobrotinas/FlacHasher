using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Andy.FlacHash.Hashfile.Read
{
    public interface IHashEntryParser
    {
        KeyValuePair<string, string> Parse(string line);
    }

    public class HashEntryParser : IHashEntryParser
    {
        private readonly Regex regex;

        /// <param name="separator">Any separator goes, except for empty value and new-line characters</param>
        public HashEntryParser(string separator)
        {
            if (string.IsNullOrEmpty(separator))
                throw new ArgumentNullException(nameof(separator), "A separator cannot be an empty value");

            if (separator.Contains("\r") || separator.Contains("\n") || separator.Contains("\r\n"))
                throw new ArgumentException(nameof(separator), "A separator cannot be a New-line value");

            if (separator.Contains("\"") || separator.Contains("'"))
                throw new ArgumentException(nameof(separator), "A separator cannot contain quotes");

            // Captures Key and Value from a string separated by a separator-char-sequence, ignoring said sequence if it's between quotes (thus treating it as part of the segment's value).
            // In other words, either segment may be wrapped in quotes, and what's between quotes gets treated as part of a value, not as a value-separator.
            // The 2nd separator at the end is to make it ignore extra segments when there's more than one separator.
            this.regex = new Regex($@"^(?<key>""[^""]*""|[^""]*?)\s*{Regex.Escape(separator)}\s*(?<value>""[^""]*""|[^""]*?)($|{Regex.Escape(separator)})", RegexOptions.ExplicitCapture);
        }

        /// <summary>
        /// If no segment separator char/sequence is found in the <paramref name="line"/>, it returns the whole line as Value and null for Key.
        /// If a segment separator char/sequence is present in the <paramref name="line"/>, it splits the the line into two segments
        /// and returns the 1st segment as Key and the 2nd one as as Value.
        /// If the separator char/sequence is found more than once, it returns first two segments omitting the rest.
        /// 
        /// Segments wrapped in quotes can safely contain separator chars, which then get treated as part of the value.
        /// </summary>
        public KeyValuePair<string, string> Parse(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (string.IsNullOrWhiteSpace(line)) throw new ArgumentException("An empty string is unacceptable!", nameof(line));
            if (string.IsNullOrWhiteSpace(line.Trim('\"'))) throw new ArgumentException("An empty string wrapped in quotes is unacceptable!", nameof(line));

            var reasult = this.regex.Match(line);

            if (!reasult.Success)
                return new KeyValuePair<string, string>(
                    null,
                    TrimAndReplaceEmptyWithNull(line));

            var segment1 = reasult.Groups["key"].Value;
            var segment2 = reasult.Groups["value"].Value;

            var key = TrimAndReplaceEmptyWithNull(segment1);
            var value = TrimAndReplaceEmptyWithNull(segment2);

            if (key == null && value == null)
                throw new ArgumentException("An empty string that only contains separator chars is unacceptable!", nameof(line));

            return new KeyValuePair<string, string>(key, value);
        }

        private string TrimAndReplaceEmptyWithNull(string value)
        {
            var trimmed = value?.Trim()?.Trim('\"').Trim();

            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}