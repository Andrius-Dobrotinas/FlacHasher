using Andy.Configuration.Ini.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.Configuration.Ini
{
    public interface IIniFileReader
    {
        /// <summary>
        /// The result is a dictionary of sections; each section is a dictionary.
        /// Root section key is Empty String.
        /// </summary>
        IDictionary<string, IDictionary<string, string>> Read(FileInfo iniFile);
    }

    public class IniFileReader : IIniFileReader
    {
        private readonly ITextFileReader fileReader;
        private readonly IIniParser parser;

        public IniFileReader(
            ITextFileReader fileReader,
            IIniParser parser)
        {
            this.fileReader = fileReader;
            this.parser = parser;
        }

        public IDictionary<string, IDictionary<string, string>> Read(FileInfo file)
        {
            string[] lines = fileReader.ReadAllLines(file);

            return parser.Parse(lines);
        }

        public static class Default
        {
            /// <summary>
            /// Reads a given INI <paramref name="settingsFile"/> using the default implementation.
            /// </summary>
            public static IDictionary<string, IDictionary<string, string>> ReadIniFile(FileInfo settingsFile)
            {
                var iniReader = new IniFileReader(
                    new TextFileReader(),
                    new IniParser(
                        new EntryParser()));

                return iniReader.Read(settingsFile);
            }
        }
    }
}