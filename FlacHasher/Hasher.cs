using FlacHasher.Crypto;
using System;
using System.IO;

namespace FlacHasher
{
    public class Hasher
    {
        private readonly IAudioDecoder decoder;
        private readonly IHashComputer hashComputer;

        public Hasher(IAudioDecoder decoder, IHashComputer hashComputer)
        {
            this.decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
            this.hashComputer = hashComputer ?? throw new ArgumentNullException(nameof(hashComputer));
        }

        public byte[] ComputerHash(string sourceFilePath)
        {
            Stream decodedData = decoder.Decode(sourceFilePath);

            return hashComputer.ComputeHash(decodedData);
        }
    }
}