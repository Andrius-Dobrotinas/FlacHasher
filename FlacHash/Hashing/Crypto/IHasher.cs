using System;
using System.IO;

namespace Andy.FlacHash.Hashing.Crypto
{
    public interface IHasher
    {
        byte[] ComputeHash(Stream data);
    }
}
