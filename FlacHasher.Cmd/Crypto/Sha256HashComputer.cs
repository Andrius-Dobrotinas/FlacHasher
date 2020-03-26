using System;
using System.IO;

namespace Andy.FlacHash.Crypto
{
    public class Sha256HashComputer : IHashComputer
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
