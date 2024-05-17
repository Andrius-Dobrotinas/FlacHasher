using Andy.FlacHash.Crypto;
using System;
using System.IO;
using System.Threading;

namespace Andy.FlacHash
{
    public interface IFileHasher
    {
        byte[] ComputerHash(FileInfo sourceFile, CancellationToken cancellation);
    }

    public class FileHasher : IFileHasher
    {
        private readonly IO.IFileReader reader;
        private readonly IHashComputer hashComputer;

        public FileHasher(IO.IFileReader reader, IHashComputer hashComputer)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.hashComputer = hashComputer ?? throw new ArgumentNullException(nameof(hashComputer));
        }

        public byte[] ComputerHash(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            using (Stream contents = reader.Read(sourceFile, cancellation))
            {
                return hashComputer.ComputeHash(contents);
            }
        }
    }
}