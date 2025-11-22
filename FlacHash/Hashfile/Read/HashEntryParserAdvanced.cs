using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Andy.FlacHash.Hashfile.Read
{
    public class HashEntryParserAdvanced
    {
        private const string GroupNameFilename = "filename";
        private const string GroupNameSeparator = "sep";

        private const string PrefixChars = "#-+*<=>";

        /// <summary>
        /// Characters that can form separator clusters between filename and hash.
        /// </summary>
        private const string SeparatorChars = "-+*<>=|#";

        // "-" needs to be escaped when it's used in a group ([]); others are fine like that
        private static readonly string SeparatorCharClass = $"\\{SeparatorChars}";

        /// <summary>
        /// Parses text that ends with a separator token (precedes the hash).
        /// Captures the leading non-whitespace text as the filename (group "filename")
        /// and the final separator characters (any combination of chars from <see cref="SeparatorChars"/>)
        /// as the separator (group "sep").
        /// Examples: "file.flac --", "Nirvana - MV  -", "track.flac <--->".
        /// </summary>
        private static readonly Regex LeadingFilenameWithSeparatorRegex = new Regex(
            @"^(?<" + GroupNameFilename + @">.*\S)\s+(?<" + GroupNameSeparator + @">[" + SeparatorCharClass + @"]+)$",
            RegexOptions.Compiled);

        /// <summary>
        /// Parses text that starts with a separator token (follows the hash).
        /// Captures the leading separator characters (any combination of chars from <see cref="SeparatorChars"/>)
        /// as the separator (group "sep") and the remaining non-whitespace text as the filename (group "filename").
        /// Examples: "-- track.flac", "-> Nirvana - MV.flac", "<---> track.flac".
        /// </summary>
        private static readonly Regex TrailingFilenameWithSeparatorRegex = new Regex(
            @"^(?<" + GroupNameSeparator + @">[" + SeparatorCharClass + @"]+)\s+(?<" + GroupNameFilename + @">.*\S)$",
            RegexOptions.Compiled);

        /// <summary>
        /// Matches a single hexadecimal hash of at least 8 bytes
        /// that is delimited by the start/end of the line or whitespace.
        /// Captures the hash in capture group 2 (index 2).
        /// </summary>
        private static readonly Regex HashRegex = new Regex(@"(^|\s)([0-9A-Fa-f]{8,})(?=$|\s)", RegexOptions.Compiled);

        /// <summary>
        /// Detects invalid separator structures: any contiguous sequence of two or more separator characters (any from <see cref="SeparatorChars"/>)
        /// where either the character immediately before the sequence is non-whitespace,
        /// or the character immediately after the sequence is non-whitespace.
        /// Clusters fully surrounded by whitespace (e.g. "\t<--->  ") are considered valid.
        /// </summary>
        private static readonly Regex InvalidSeparatorRegex = new Regex(
            @"(?<![" + SeparatorCharClass + @"])(?<!\s)[" + SeparatorCharClass + @"]{2,}|[" + SeparatorCharClass + @"]{2,}(?!\s)(?![" + SeparatorCharClass + @"])",
            RegexOptions.Compiled);

        public KeyValuePair<string, string>? Parse(string line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                return null;

            var body = StripPrefix(trimmed);
            if (string.IsNullOrWhiteSpace(body))
                return null;

            if (InvalidSeparatorRegex.IsMatch(body))
                throw new InvalidHashLineFormatException("Invalid separator structure found");

            var hashMatches = HashRegex.Matches(body);

            if (hashMatches.Count == 0)
                return null;

            if (hashMatches.Count > 1)
                throw new InvalidHashLineFormatException("Multiple hashes found");

            var match = hashMatches.First();
            var hashGroup = match.Groups[2];
            var hash = hashGroup.Value;

            var hashStartIndex = hashGroup.Index;
            var hashEndIndex = hashStartIndex + hashGroup.Length;

            var textBeforeHash = body.Substring(0, hashStartIndex).Trim();
            var textAfterHash = body.Substring(hashEndIndex).Trim();

            if (textBeforeHash.Length == 0 && textAfterHash.Length == 0)
                return new KeyValuePair<string, string>(null, hash);

            if (textBeforeHash.Length == 0)
            {
                var file = ExtractFilenameFromTrailingText(textAfterHash);
                if (string.IsNullOrWhiteSpace(file))
                    return null;

                return new KeyValuePair<string, string>(file, hash);
            }
            else
            {
                var file = ExtractFilenameFromLeadingText(textBeforeHash);
                if (string.IsNullOrWhiteSpace(file))
                    return null;

                return new KeyValuePair<string, string>(file, hash);
            }
        }

        private static string StripPrefix(string value)
        {
            var s = value;
            var i = 0;

            while (i < s.Length && PrefixChars.Contains(s[i]))
                i++;

            if (i == 0)
                return s;

            while (i < s.Length && char.IsWhiteSpace(s[i]))
                i++;

            return s.Substring(i);
        }

        private static string ExtractFilenameFromLeadingText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var match = LeadingFilenameWithSeparatorRegex.Match(text);
            if (match.Success)
            {
                var filename = match.Groups[GroupNameFilename].Value;
                return filename.Length == 0 ? null : filename;
            }

            return text;
        }

        private static string ExtractFilenameFromTrailingText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var match = TrailingFilenameWithSeparatorRegex.Match(text);
            if (match.Success)
            {
                var filename = match.Groups[GroupNameFilename].Value;
                return filename.Length == 0 ? null : filename;
            }

            return text;
        }
    }
}
