using System.IO;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Builds a runner the way the fake-process tests want one: a timeout given in seconds, as those tests reason in,
    /// and the default error-output limit.
    /// </summary>
    static class TestRunner
    {
        public static ProcessRunner WithTimeoutInSeconds(int timeoutSec, int exitTimeoutMs = 0, int startWaitMs = 0, bool showProcessOutput = false)
        {
            return new ProcessRunner(
                ProcessRunner.TimeoutFromSeconds(timeoutSec),
                exitTimeoutMs,
                startWaitMs,
                ProcessRunner.DefaultMaxErrorOutputBytes,
                showProcessOutput);
        }
    }
}
