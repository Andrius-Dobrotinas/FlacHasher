﻿using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Hashfile.Read
{
    public class MissingFileNameException : HashEntryException
    {
        public MissingFileNameException(int entryNumber) : base($"Hash entry is missing a file name. Entry number: {entryNumber}")
        {
        }
    }
}