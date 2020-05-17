using Andy.FlacHash.Audio.Compression;
using System;
using System.IO;

namespace Andy.FlacHash
{
    class Program
    {
        const uint maxCompressionLevel = 8;

        static void Main(string[] args)
        {
            var flacExe = new FileInfo(args[0]);
            var sourceFile = new FileInfo(args[1]);

            var recoder = new CmdLineFlacRecoder(flacExe, maxCompressionLevel);

            using (MemoryStream recodedAudio = recoder.Encode(sourceFile))
            {
                Console.WriteLine($"{sourceFile.FullName}: original size: {sourceFile.Length}, level {maxCompressionLevel} size: {recodedAudio.Length}");
            }
        }
    }
}