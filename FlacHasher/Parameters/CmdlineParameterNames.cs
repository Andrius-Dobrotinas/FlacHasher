namespace Andy.FlacHash.Application
{
    public static class CmdlineParameterNames
    {
        public const string Profile = "--profile";
        public const string DecoderProfile = "--profile-decoder";
        public const string HashingProfile = "--profile-hashing";
        public const string OutputFormat = "--format";
        public const string Decoder = "--decoder";
        public const string DecoderPrintProgress = "--decoder-verbose";
        public const string HashAlgorithm = "--algorithm";

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
        public const string ModeHelp = "help";
        public const string HashFile = "--hash";
    }
}
