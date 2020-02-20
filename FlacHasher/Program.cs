using FlacHasher.Crypto;
using System;

namespace FlacHasher
{
    class Program
    {
        static void Main(string[] args)
        {
            var encoderExecutablePath = @"C:\Program Files (x86)\FLAC Frontend\tools\flac.exe";
            var sourceFilePath = @"c:\01 - Hex Me.flac";

            var hasher = new Hasher(encoderExecutablePath, new Sha256HashComputer());

            byte[] hash = hasher.ComputerHash(sourceFilePath);

            Console.WriteLine(BitConverter.ToString(hash));
        }
    }
}