using FlacHasher.Crypto;
using System;
using System.IO;

namespace FlacHasher
{
    public class Hasher
    {
        private readonly IFileReader decoder;
        private readonly IHashComputer hashComputer;

        public Hasher(IFileReader decoder, IHashComputer hashComputer)
        {
            this.decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
            this.hashComputer = hashComputer ?? throw new ArgumentNullException(nameof(hashComputer));
        }

        public byte[] ComputerHash(FileInfo sourceFile)
        {
            Stream decodedData = decoder.Read(sourceFile);

            return hashComputer.ComputeHash(decodedData);
        }
    }
}