using Andy.FlacHash.Audio;
using Andy.FlacHash.Hashing.Crypto;
using System;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.Hashing
{
    public interface IFileHasher
    {
        byte[] ComputeHash(FileInfo sourceFile, CancellationToken cancellation);
    }

    public class FileHasher : IFileHasher
    {
        private readonly IAudioFileDecoder fileDecoder;
        private readonly IHashComputer hashComputer;

        public FileHasher(IAudioFileDecoder decoder, IHashComputer hashComputer)
        {
            fileDecoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
            this.hashComputer = hashComputer ?? throw new ArgumentNullException(nameof(hashComputer));
        }

        public byte[] ComputeHash(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            using (Stream contents = fileDecoder.Read(sourceFile, cancellation))
            {
                return hashComputer.ComputeHash(contents);
            }
        }
    }
}