﻿using Andy.FlacHash.Crypto;
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
        private readonly IO.IFileDecoder fileDecoder;
        private readonly IHashComputer hashComputer;

        public FileHasher(IO.IFileDecoder decoder, IHashComputer hashComputer)
        {
            this.fileDecoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
            this.hashComputer = hashComputer ?? throw new ArgumentNullException(nameof(hashComputer));
        }

        public byte[] ComputerHash(FileInfo sourceFile, CancellationToken cancellation = default)
        {
            using (Stream contents = fileDecoder.Read(sourceFile, cancellation))
            {
                return hashComputer.ComputeHash(contents);
            }
        }
    }
}