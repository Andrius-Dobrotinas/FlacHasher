using System;

namespace Andy.FlacHash.Verification
{
    public class HashEntryException : Exception
    {
        public HashEntryException(string msg) : base(msg)
        {

        }
    }

    public class MissingFileNameException : HashEntryException
    {
        public MissingFileNameException() : base("Hash entry is missing a file name")
        {

        }
    }

    public class MissingHashValueException : HashEntryException
    {
        public MissingHashValueException() : base("An entry is missing a hash value")
        {

        }
    }

    public class DuplicateFileException : Exception
    {
        public DuplicateFileException() : base("File name is repeated in the file")
        {

        }
    }
}