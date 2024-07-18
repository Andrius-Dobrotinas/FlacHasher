using System;
using System.IO;
using System.Security.Cryptography;

namespace Andy.FlacHash.Crypto
{
    public class HashComputer : IHashComputer
    {
        private readonly string algo;

        public HashComputer(Algorithm algorithm)
        {
            this.algo = algorithm.ToString();
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