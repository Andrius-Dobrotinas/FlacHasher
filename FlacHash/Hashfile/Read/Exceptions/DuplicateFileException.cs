﻿using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Hashfile.Read
{
    public class DuplicateFileException : Exception
    {
        public DuplicateFileException(string fileName, int entryNumber) : base($"File name is repeated: {fileName}. Entry number {entryNumber}")
        {

        }
    }
}
