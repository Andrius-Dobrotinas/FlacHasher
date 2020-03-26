using System;
using System.IO;

namespace Andy.FlacHash.Crypto
{
    public interface IHashComputer
    {
        byte[] ComputeHash(Stream data);
    }
}
