using System.IO;

namespace Andy.FlacHash
{
    public class Parameters
    {
        public FileInfo Decoder { get; set; }
        public FileInfo InputFile { get; set; }
        public string OutputFormat { get; set; }
    }
}