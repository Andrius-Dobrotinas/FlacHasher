using System.Linq;

namespace Andy.FakeDecoder
{
    /// <summary>
    /// The flag list the parsing tests are driven off, so that a flag added to the program can't slip past them.
    /// </summary>
    static class Flags
    {
        const string valuelessFlag = "--stdin";

        public static readonly string[] All =
        {
            "--file",
            valuelessFlag,
            "--xor",
            "--expand",
            "--read-chunk-size",
            "--write-delay",
            "--finish-after-reads",
            "--progress-message",
            "--success-message",
            "--error-message",
            "--keep-stdout-open",
            "--linger",
            "--exit-code"
        };

        public static readonly string[] TakingAValue = All.Where(x => x != valuelessFlag).ToArray();
    }
}
