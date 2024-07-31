using System;

namespace Andy.FlacHash.Hashing.Verification
{
    public class HashEntryException : Exception
    {
        public HashEntryException(string msg) : base(msg)
        {

        }
    }

    public class MissingFileNameException : HashEntryException
    {
        public MissingFileNameException(int entryNumber) : base($"Hash entry is missing a file name. Entry number: {entryNumber}")
        {
        }
    }

    public class MissingHashValueException : HashEntryException
    {
        public MissingHashValueException(int entryNumber) : base($"An entry is missing a hash value. Entry number: {entryNumber}")
        {

        }
    }

    public class DuplicateFileException : Exception
    {
        public DuplicateFileException(string fileName, int entryNumber) : base($"File name is repeated: {fileName}. Entry number {entryNumber}")
        {

        }
    }
}