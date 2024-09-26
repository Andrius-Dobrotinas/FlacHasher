using Andy.Configuration.Ini.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.Configuration.Ini
{
    public interface IIniFileReader
    {
        /// <summary>
        /// Root section key is Empty String
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
    }
}