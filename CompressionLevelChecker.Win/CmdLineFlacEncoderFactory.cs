using Andy.FlacHash.Audio.Compression;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public class CmdLineFlacEncoderFactory
    {
        private readonly FileInfo flacExe;

        public CmdLineFlacEncoderFactory(FileInfo flacExe)
        {
            this.flacExe = flacExe;
        }

        public CmdLineFlacRecoder Build(uint compressionLevel)
        {
            return new CmdLineFlacRecoder(flacExe, compressionLevel);
        }
    }
}