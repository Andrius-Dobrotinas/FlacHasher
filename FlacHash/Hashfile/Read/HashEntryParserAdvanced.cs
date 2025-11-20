using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Andy.FlacHash.Hashfile.Read
{
    public class HashEntryParserAdvanced
    {
        private const string GroupNameFilename = "filename";
        private const string GroupNameSeparator = "sep";

        private static readonly HashSet<string> Separators = new HashSet<string>(StringComparer.Ordinal)
        {
            ">", "<", "=", "-", "*", "|", "--", "->", "=>", ">>", "||"
        };

        private static bool IsSeparatorToken(string token) => Separators.Contains(token);

        private const string PrefixChars = "#-+*<=>";

        /// <summary>
        /// Parses text that ends with a separator token.
        /// Captures the leading non-whitespace text as the filename (group "filename")
        /// and the final 1-2 separator characters as the separator (group "sep").
        /// Examples: "file.flac --", "Nirvana - MV  -".
        /// </summary>
        private static readonly Regex LeadingFilenameWithSeparatorRegex = new Regex(@"^(?<" + GroupNameFilename + @">.*\S)\s+(?<" + GroupNameSeparator + @">[\-+*<>=|]{1,2})$", RegexOptions.Compiled);

        /// <summary>
        /// Parses text that starts with a separator token.
        /// Captures the first 1-2 separator characters as the separator (group "sep")
        /// and the remaining non-whitespace text as the filename (group "filename").
        /// Examples: "-- track.flac", "-> Nirvana - MV.flac".
        /// </summary>
        private static readonly Regex TrailingFilenameWithSeparatorRegex = new Regex(@"^(?<" + GroupNameSeparator + @">[\-+*<>=|]{1,2})\s+(?<" + GroupNameFilename + @">.*\S)$", RegexOptions.Compiled);

        /// <summary>
        /// Matches a single hexadecimal hash of at least 8 bytes
        /// that is delimited by the start/end of the line or whitespace.
        /// Captures the hash in capture group 2 (index 2).
        /// </summary>
        private static readonly Regex HashRegex = new Regex(@"(^|\s)([0-9A-Fa-f]{8,})(?=$|\s)", RegexOptions.Compiled);

        /// <summary>
        /// Detects invalid separator structures: any sequence of two or more separator
        /// characters (-, +, *, <, >, =, |) that is not surrounded by whitespace on both sides.
        /// It's used to reject lines where separators are glued to adjacent text,
        /// e.g. "slts.flac--DEADBEAF00112233" or "DEADBEAF00112233** slts.flac".
        /// </summary>
        private static readonly Regex InvalidSeparatorRegex = new Regex(@"(?<!\s)[\-+*<>=|]{2,}|[\-+*<>=|]{2,}(?!\s)", RegexOptions.Compiled);

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
            if (match.Success && IsSeparatorToken(match.Groups[GroupNameSeparator].Value))
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
            if (match.Success && IsSeparatorToken(match.Groups[GroupNameSeparator].Value))
            {
                var filename = match.Groups[GroupNameFilename].Value;
                return filename.Length == 0 ? null : filename;
            }

            return text;
        }
    }
}
