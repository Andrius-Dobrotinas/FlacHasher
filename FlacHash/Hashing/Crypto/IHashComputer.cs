using System;
using System.IO;

namespace Andy.FlacHash.Hashing.Crypto
{
    public interface IHashComputer
    {
        byte[] ComputeHash(Stream data);
    }
}
