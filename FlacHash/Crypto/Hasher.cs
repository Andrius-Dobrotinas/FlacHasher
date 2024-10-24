using System;
using System.IO;
using System.Security.Cryptography;

namespace Andy.FlacHash.Crypto
{
    public class Hasher : IHasher
    {
        private readonly string algo;

        public Hasher(Algorithm algorithm)
        {
            algo = algorithm.ToString();
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