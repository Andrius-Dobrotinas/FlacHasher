namespace Andy.FlacHash.Hashfile.Read
{
    public interface IHashEntryParser
    {
        KeyValuePair<string, string>? Parse(string line);
    }
}