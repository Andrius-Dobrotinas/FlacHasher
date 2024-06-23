namespace Andy.FlacHash.Cmd
{
    public enum ReturnValue
    {
        Success = 0,
        ArgumentNotProvided = -1,
        ArgumentError = -2,
        NoFilesToProcess = -3,
        Cancellation = -99,
        ExecutionFailure = -100,
        InputReadingFailure = -200,
        SettingsReadingFailure = -300,
        ConfigurationError = -301
    }
}