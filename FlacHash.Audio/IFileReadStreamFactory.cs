﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Audio
{
    public interface IFileReadStreamFactory
    {
        /// <summary>
        /// Returns <paramref name="sourceFile"/> as a read-only stream
        /// </summary>
        Stream CreateStream(FileInfo sourceFile);
    }
}