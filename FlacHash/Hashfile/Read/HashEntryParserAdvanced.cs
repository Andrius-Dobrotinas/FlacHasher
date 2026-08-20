using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Andy.FlacHash.Hashfile.Read
{
    /// <summary>
    /// Parses hash file lines to extract a single hexadecimal hash and an optional filename that the hash is for.
    /// Supports hashes of at least 8 bytes (16 hex characters) -- which can't contain dashes.
    /// A hash can be wrapped in a matching pair of brackets or <see cref="AcceptedHashWrappers"/>
    /// A filename must precede the hash
    /// A filename and a hash must be separated by whitespace or a cluster of <see cref="SeparatorChars"/> surrounded by whitespace.
    /// A filename can contain any characters, as long as it contains at least one alphanumeric one.
    /// A filename can have a hash pattern, but in that case, it has to have an extension - otherwise, it will be mistreated as a hash.
    /// The line may start with whitespace or a <see cref="SeparatorChars"/> prefix, which get discarded.
    /// A line cannot contain more than one hash.
    /// 
    /// Throws <see cref="MissingHashValueException"/> for lines that don't contain a hash.
    /// Throws <see cref="InvalidHashLineFormatException"/> for malformed lines, such as those with multiple hashes, invalid separator sequences or an unusable file name.
    /// </summary>
    public class HashEntryParserAdvanced : IHashEntryParser
    {
        /// <summary>
        /// Characters that can form separator clusters between filename and hash.
        /// </summary>
        public const string SeparatorChars = "-+*<>=|#:";

        /// <summary>
        /// Hex chars, at least 16 chars long (8 bytes)
        /// </summary>
        private const string HashPattern = "[0-9A-Fa-f]{16,}";

        private const string HashGroupName = "hash";
        private static readonly string[] AcceptedHashWrappers = { "[]", "{}", "()", "``" };

        private static readonly string HashCapture = "(?<" + HashGroupName + ">" + HashPattern + ")";

        /// <summary>
        /// Matches a single hexadecimal hash of at least 8 bytes (16 hex characters)
        /// that is delimited by the start/end of the line or whitespace,
        /// either on its own or wrapped in a matching pair of brackets or backticks.
        /// Captures the hash itself, sans the wrapper, in the <see cref="HashGroupName"/> group.
        /// </summary>
        private static readonly Regex HashWordRegex = new Regex(
            "(?<=^|\\s)(?:" + HashCapture
                + "|\\[" + HashCapture + "\\]"
                + "|\\{" + HashCapture + "\\}"
                + "|\\(" + HashCapture + "\\)"
                + "|`" + HashCapture + "`"
                + ")(?=$|\\s)",
            RegexOptions.Compiled);

        public KeyValuePair<string, string>? Parse(string line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            var body = StripPrefix(line.Trim()).Trim();
            if (string.IsNullOrWhiteSpace(body))
                return null;

            var hashMatches = HashWordRegex.Matches(body);

            // No properly delimited hash found.
            if (hashMatches.Count == 0)
                throw new MissingHashValueException();

            if (hashMatches.Count > 1)
                throw new InvalidHashLineFormatException("Multiple hashes found");

            var match = hashMatches.First();
            var matchStartIndexWithinBody = match.Index;
            var file = ExtractFilename(body.Substring(0, matchStartIndexWithinBody));

            return new KeyValuePair<string, string>(file, match.Groups[HashGroupName].Value);
        }

        private static string StripPrefix(string value)
        {
            var i = 0;

            // Determine how many chars are prefix
            while (i < value.Length && SeparatorChars.Contains((char)value[i]))
                i++;

            // No prefix characters found, return the original string
            if (i == 0)
                return value;

            return value.Substring(i);
        }

        /// <summary>
        /// Reads the text preceding the hash backwards: whitespace, then an optional separator cluster
        /// (which must be detached from the file name by whitespace), then the file name itself.
        /// </summary>
        private static string ExtractFilename(string text)
        {
            var end = SkipBackwards(text, text.Length, char.IsWhiteSpace);
            var touchesTheHash = end == text.Length;

            var separatorStart = SkipBackwards(text, end, SeparatorChars.Contains);
            if (separatorStart < end)
            {
                if (touchesTheHash)
                    throw new InvalidHashLineFormatException("A separator must be detached from a hash by whitespace");

                if (separatorStart > 0 && !char.IsWhiteSpace(text[separatorStart - 1]))
                    throw new InvalidHashLineFormatException("A separator must be detached from a file name by whitespace");

                end = SkipBackwards(text, separatorStart, char.IsWhiteSpace);
            }

            if (end == 0)
                return null;

            var filename = text.Substring(0, end);

            if (!filename.Any(char.IsLetterOrDigit))
                throw new InvalidHashLineFormatException($"A file name must contain alphanumeric characters: \"{filename}\"");

            return filename;
        }

        /// <summary>
        /// Returns the index of the first char, going backwards from <paramref name="end"/>, that doesn't satisfy the <paramref name="predicate"/>.
        /// </summary>
        private static int SkipBackwards(string text, int end, Func<char, bool> predicate)
        {
            while (end > 0 && predicate(text[end - 1]))
                end--;

            return end;
        }
    }
}
