using FlacHasher.Crypto;
using System;
using System.IO;

namespace FlacHasher
{
    class Program
    {
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

            var hasher = new Hasher(encoderExecutable.FullName, new Sha256HashComputer());

            byte[] hash = hasher.ComputerHash(sourceFile.FullName);

            Console.WriteLine(BitConverter.ToString(hash));

            return 0;
        }
    }
}