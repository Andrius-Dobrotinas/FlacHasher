namespace Andy.FlacHash.Application.Cmd.E2E
{
    static class HashCommand
    {
        // Decode to std-out, reading the file from std-in
        public static readonly string[] FlacStreamDecoderParams = { "--decode", "-" };

        public static string[] Arguments(FileInfo inputFile, FileInfo decoder, string algorithm, string[] decoderParams, string outputFormat = null)
        {
            return Arguments(new[] { inputFile }, decoder, algorithm, decoderParams, outputFormat);
        }

        public static string[] Arguments(IEnumerable<FileInfo> inputFiles, FileInfo decoder, string algorithm, string[] decoderParams, string outputFormat = null)
        {
            var arguments = new List<string>
            {
                "hash",
                $"--decoder={decoder.FullName}",
                $"--algorithm={algorithm}",
                "--process-timeout=30",
                "--decoder-verbose=false"
            };

            arguments.AddRange(inputFiles.Select(x => $"--input={x.FullName}"));
            arguments.AddRange(decoderParams.Select(x => $"--params={x}"));

            if (outputFormat != null)
                arguments.Add($"--format={outputFormat}");

            return arguments.ToArray();
        }
    }
}
