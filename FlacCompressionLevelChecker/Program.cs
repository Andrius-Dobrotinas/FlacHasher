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
            var compressionLevel = uint.Parse(args[2]);

            var recoder = new CmdLineFlacRecoder(flacExe, compressionLevel);

            using (MemoryStream recodedAudio = recoder.Encode(sourceFile))
            {
                Console.WriteLine($"{sourceFile.FullName}: compressed to level {compressionLevel}: {sourceFile.Length == recodedAudio.Length}");
            }
        }
    }
}