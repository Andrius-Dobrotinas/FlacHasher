namespace Andy.FlacHash.Hashfile.Read
{
    public class HashFileException : Exception
    {
        public HashFileException(string msg) : base(msg)
        {
        }

        public HashFileException(string msg, Exception exception) : base(msg, exception)
        {
        }
    }
}