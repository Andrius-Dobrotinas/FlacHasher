using System;
using System.IO;

namespace Andy.FlacHash.Crypto
{
    public class Sha256HashComputer : IHashComputer
    {
        public byte[] ComputeHash(Stream data)
        {
            using (var algorithm = System.Security.Cryptography.HashAlgorithm.Create(typeof(System.Security.Cryptography.SHA256Managed).FullName))
            {
                return algorithm.ComputeHash(data);
            }
        }
    }
}
