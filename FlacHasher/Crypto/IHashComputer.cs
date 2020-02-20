using System;
using System.IO;

namespace FlacHasher.Crypto
{
    public interface IHashComputer
    {
        byte[] ComputeHash(Stream data);
    }
}
