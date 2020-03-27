﻿using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public class Parameters
    {
        public FileInfo Decoder { get; set; }
        public IReadOnlyCollection<FileInfo> InputFiles { get; set; }
        public string OutputFormat { get; set; }
    }
}