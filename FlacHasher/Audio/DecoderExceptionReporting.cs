using Andy.ExternalProcess;
using Andy.FlacHash.Audio;

namespace Andy.FlacHash.Application.Audio
{
    /// <summary>
    /// Explains why a decoder failure might have happened, so both applications give the same explanation for the same failure.
    /// </summary>
    public static class DecoderExceptionReporting
    {
        public static string GetPossibleReason(DecoderException exception) => exception.ActualException switch
        {
            ProcessNotRespondingException _ => "misconfiguration/incorrect parameters, or a problem with the operating system.",
            ProcessTimeoutException _ => "the file is unusually large, the decoder is misconfigured/using incorrect parameters or stopped responding.",
            PrematureExitException _ => "the decoder is misconfigured/using incorrect parameters, or the input file is corrupt.",
            _ => "the file may be corrupt, wrong format or decoder is misconfigured/incorrect parameters."
        };
    }
}
