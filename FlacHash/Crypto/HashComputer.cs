using System;
using System.IO;
using System.Security.Cryptography;

namespace Andy.FlacHash.Crypto
{
    public class HashComputer : IHashComputer
    {
        private readonly string algo;

        public HashComputer(string algorithm)
        {
            if (algorithm == null)
                throw new ArgumentNullException(nameof(algorithm));

            if (algorithm != nameof(MD5) && algorithm != nameof(SHA1) && algorithm != nameof(SHA256) && algorithm != nameof(SHA512))
                throw new ArgumentException(nameof(algorithm), $"Valid algorithm values are: {nameof(MD5)}, {nameof(SHA1)}, {nameof(SHA256)}, {nameof(SHA512)}");

            this.algo = algorithm;
        }

        public byte[] ComputeHash(Stream data)
        {
            using (var algorithm = HashAlgorithm.Create(algo))
            {
                return algorithm.ComputeHash(data);
            }
        }
    }
}
