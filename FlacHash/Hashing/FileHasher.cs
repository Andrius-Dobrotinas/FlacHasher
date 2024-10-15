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
        private readonly IHasher hasher;

        public FileHasher(IAudioFileDecoder decoder, IHasher hasher)
        {
            fileDecoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public byte[] ComputeHash(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            using (Stream contents = fileDecoder.Read(sourceFile, cancellation))
            {
                return hasher.ComputeHash(contents);
            }
        }
    }
}