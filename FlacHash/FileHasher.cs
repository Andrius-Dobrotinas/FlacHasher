using Andy.FlacHash.Crypto;
using System;
using System.IO;

namespace Andy.FlacHash
{
    public class FileHasher : IFileHasher
    {
        private readonly Input.IFileReader reader;
        private readonly IHashComputer hashComputer;

        public FileHasher(Input.IFileReader reader, IHashComputer hashComputer)
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