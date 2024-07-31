namespace Andy.FlacHash.Hashing
{
    public interface IHashFormatter
    {
        string GetString(byte[] hash);
    }
}