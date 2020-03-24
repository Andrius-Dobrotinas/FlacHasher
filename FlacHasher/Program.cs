using FlacHasher.Crypto;
using System;
using System.IO;
using System.Linq;

namespace FlacHasher
{
    class Program
    {
        private const string newLine = "\r\n";
        private const string optionName_stdOut = "--stdout";

        static int Main(string[] args)
        {
            var encoderExecutable = new FileInfo(@"C:\Program Files (x86)\FLAC Frontend\tools\flac.exe");
            if (!encoderExecutable.Exists)
            {
                Console.WriteLine("The encoder executable file doesn't exist");
                return -1;
            }

            var sourceFile = new FileInfo(@"c:\01 - Hex Me.flac");
            if (!sourceFile.Exists)
            {
                Console.WriteLine("The source file doesn't exist");
                return -1;
            }

            var decoder = new FlacDecoder(encoderExecutable);
            var hasher = new Hasher(decoder, new Sha256HashComputer());

            byte[] hash = hasher.ComputerHash(sourceFile);

            var outputToStdOut = args.Contains(optionName_stdOut);
            if (outputToStdOut)
            {
                Console.OpenStandardOutput().Write(hash, 0, hash.Length);
                Console.Error.Write(newLine);
            }
            else
            {
                Console.WriteLine(BitConverter.ToString(hash));
            }

            Console.Error.WriteLine("Done!");

            return 0;
        }
    }
}