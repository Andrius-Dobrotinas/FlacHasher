using System;
using System.IO;

namespace FlacHasher.Crypto
{
    public class Sha256HashComputer
    {
        public byte[] ComputeHash(Stream data)
        {
            using (var algorithm = System.Security.Cryptography.HashAlgorithm.Create("System.Security.Cryptography.SHA256Managed"))
            {
                return algorithm.ComputeHash(data);
            }
        }
    }
}
