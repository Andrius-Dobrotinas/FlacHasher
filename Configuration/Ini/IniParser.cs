using System;
using System.Collections.Generic;

namespace Andy.Configuration.Ini
{
    public interface IIniParser
    {
        IDictionary<string, IDictionary<string, string>> Parse(IEnumerable<string> lines);
    }

    public class IniParser : IIniParser
    {
        public static readonly string DefaultSectionName = "";
        
        private readonly IEntryParser entryParser;

        public IniParser(
            IEntryParser entryParser)
        {
            this.entryParser = entryParser;
        }

        public IDictionary<string, IDictionary<string, string>> Parse(IEnumerable<string> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));

            var result = new Dictionary<string, IDictionary<string, string>>();

            Dictionary<string, string> currentSection = null;

            foreach (var line in lines)
            {
                if  (line.StartsWith('['))
                {
                    // Technically, should check that a section definition is correctly closed, but I don't see any harm in simply accepting that.
                    var sectionName = line.TrimStart('[').TrimEnd(']')
                        .Trim();

                    // TODO: maybe simply silently ignore that?
                    if (string.IsNullOrWhiteSpace(sectionName)) throw new InvalidSectionDeclaration(line);

                    // TODO: do I want to fobid spaces in section names?

                    currentSection = new Dictionary<string, string>();
                    result.Add(sectionName, currentSection);
                }
                else if (IsConfigEntry(line))
                {
                    var keyValuePair = entryParser.Parse(line);

                    if (keyValuePair != null)
                    {
                        if (currentSection == null)
                        {
                            currentSection = new Dictionary<string, string>();
                            result.Add(DefaultSectionName, currentSection);
                        }

                        currentSection.Add(keyValuePair.Value.Key, keyValuePair.Value.Value);
                    }
                }
            }

            return result;
        }

        private static bool IsConfigEntry(string line)
        {
            var trimmedLine = line.Trim();
            return trimmedLine != "" && !trimmedLine.StartsWith(';');
        }
    }
}