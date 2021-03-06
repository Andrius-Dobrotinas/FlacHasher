﻿using System;
using System.IO;

namespace Andy.Configuration.Ini.IO
{
    public interface ITextFileReader
    {
        string[] ReadAllLines(FileInfo file);
    }

    public class TextFileReader : ITextFileReader
    {
        public string[] ReadAllLines(FileInfo file)
        {
            return File.ReadAllLines(file.FullName);
        }
    }
}