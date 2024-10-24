using System;
using System.IO;

namespace Andy.FlacHash.Crypto
{
    public interface IHasher
    {
        byte[] ComputeHash(Stream data);
    }
}
