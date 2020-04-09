namespace Andy.FlacHash.Cmd
{
    public enum ReturnValue
    {
        Success = 0,
        ArgumentNotFound = -1,
        ArgumentError = -2,
        ExecutionFailure = -100,
        InputReadingFailure = -200,
        SettingsReadingFailure = -300,
        ConfigurationError = -301
    }
}