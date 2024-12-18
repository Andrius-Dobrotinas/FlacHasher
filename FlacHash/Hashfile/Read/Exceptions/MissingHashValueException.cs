﻿using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Hashfile.Read
{
    public class MissingHashValueException : HashEntryException
    {
        public MissingHashValueException(int entryNumber) : base($"An entry is missing a hash value. Entry number: {entryNumber}")
        {

        }
    }
}
