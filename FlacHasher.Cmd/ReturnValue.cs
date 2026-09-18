namespace Andy.FlacHash.Application.Cmd
{
    // Codes must fit in 1-127: POSIX exit statuses are an unsigned byte, and 128+ is conventionally "killed by signal".
    public enum ReturnValue
    {
        Success = 0,
        ArgumentNotProvided = 1,
        ArgumentError = 2,
        NoFilesToProcess = 3,
        Cancellation = 9,
        ExecutionFailure = 10,
        InputReadingFailure = 20,
        SettingsReadingFailure = 30,
        ConfigurationError = 31
    }
}