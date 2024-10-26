﻿using System;
using System.Collections.Generic;

namespace Andy.FlacHash
{
    public class FileHashMap
    {
        public IList<KeyValuePair<string, string>> Hashes { get; }
        public bool IsPositionBased { get; }

        public FileHashMap(IList<KeyValuePair<string, string>> hashes, bool hasNoFileNames)
        {
            Hashes = hashes;
            IsPositionBased = hasNoFileNames;
        }
    }
}