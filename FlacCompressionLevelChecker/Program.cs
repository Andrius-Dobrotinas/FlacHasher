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
            var file = new FileInfo(args[1]);

            var originalSize = file.Length;

            IFileReader reader = new Input.Flac.CmdLineDecoder(flacExe);
            var encoder = new CmdLineFlacEncoder(flacExe, maxCompressionLevel);

            using (Stream rawAudio = reader.Read(file))
            {
                rawAudio.Seek(0, SeekOrigin.Begin);

                using (MemoryStream encodedAudio = encoder.Encode(rawAudio))
                    Console.WriteLine($"{file.FullName}: original size: {originalSize}, level 8 size: {encodedAudio.Length}");
            }
        }
    }
}