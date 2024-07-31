﻿using Andy.FlacHash.Hashing.Crypto;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public class Settings
    {
        public FileInfo Decoder { get; set; }
        public string HashfileExtensions { get; set; }
        public string HashfileEntrySeparator { get; set; }
        public string HashAlgorithm { get; set; }
        public bool FileLookupIncludeHidden { get; set; }
        public string OutputFormat { get; set; }
        public int? ProcessExitTimeoutMs { get; set; }
        public int? ProcessTimeoutSec { get; set; }
        public int? ProcessStartWaitMs { get; set; }
        public bool? FailOnError { get; set; }

        public static class Defaults
        {
            public static Algorithm HashAlgorithm = Algorithm.SHA256;
        }
    }
}