using FlacHasher.Crypto;
using System;
using System.IO;

namespace FlacHasher
{
    public class FileHasher
    {
        private readonly IFileReader reader;
        private readonly IHashComputer hashComputer;

        public FileHasher(IFileReader reader, IHashComputer hashComputer)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.hashComputer = hashComputer ?? throw new ArgumentNullException(nameof(hashComputer));
        }

        public byte[] ComputerHash(FileInfo sourceFile)
        {
            Stream contents = reader.Read(sourceFile);

            return hashComputer.ComputeHash(contents);
        }
    }
}