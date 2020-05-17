using Andy.FlacHash.Audio.Compression;
using Andy.FlacHash.Input;
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

            IFileReader reader = new Input.Flac.CmdLineDecoder(flacExe);
            IAudioEncoder encoder = new CmdLineFlacEncoder(flacExe, maxCompressionLevel);
            var recoder = new AudioFileEncoder(reader, encoder);

            using (MemoryStream recodedAudio = recoder.Encode(sourceFile))
            {
                Console.WriteLine($"{sourceFile.FullName}: original size: {sourceFile.Length}, level {maxCompressionLevel} size: {recodedAudio.Length}");
            }
        }
    }
}