using System.IO;

namespace FlacHasher
{
    public class Parameters
    {
        public FileInfo Decoder { get; set; }
        public FileInfo InputFile { get; set; }
        public string OutputFormat { get; set; }
    }
}