namespace Andy.FlacHash.Cmd
{
    public static class ArgumentNames
    {
        public const string OutputFormat = "--format";
        public const string Decoder = "--decoder";

        /// <summary>
        /// Files will be processed in the order they're provided
        /// </summary>
        public const string InputFiles = "--input";

        public const string InputDirectory = "--inputDir";
        public const string FileExtension = "--file-extension";
        public const string ProcessExitTimeoutMs = "--process-exit-timeout";
        public const string ProcessTimeoutSec = "--process-timeout";
        public const string FailOnError = "--fail-fast";

        public const string ModeVerify = "verify";
        public const string HashFile = "--hash";
    }
}
