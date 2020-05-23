using Andy.FlacHash.Audio.Compression;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public interface ICmdLineFlacEncoderFactory
    {
        ICmdLineFlacRecoder Build(uint compressionLevel);
    }

    public class CmdLineFlacEncoderFactory : ICmdLineFlacEncoderFactory
    {
        private readonly FileInfo flacExe;

        public CmdLineFlacEncoderFactory(FileInfo flacExe)
        {
            this.flacExe = flacExe;
        }

        public ICmdLineFlacRecoder Build(uint compressionLevel)
        {
            return new CmdLineFlacRecoder(flacExe, compressionLevel);
        }
    }
}