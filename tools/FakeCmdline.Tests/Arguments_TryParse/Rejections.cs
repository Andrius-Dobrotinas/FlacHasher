using NUnit.Framework;
using System;
using System.IO;

namespace Andy.FakeCmdline
{
    public class Rejections
    {
        [Test]
        public void When__AFlagIsUnknown__Must_Reject()
        {
            AssertRejected("--decode-everything", "yes");
        }

        [TestCase("--file")]
        [TestCase("--xor")]
        [TestCase("--output-chunk-size")]
        [TestCase("--progress-message")]
        [TestCase("--linger")]
        [TestCase("--exit-code")]
        public void When__AFlagIsMissingItsValue__Must_Reject(string flag)
        {
            AssertRejected(flag);
        }

        [TestCase("--output-chunk-size")]
        [TestCase("--output-chunk-delay")]
        [TestCase("--stop-after-chunks")]
        [TestCase("--keep-stdout-open")]
        [TestCase("--linger")]
        [TestCase("--exit-code")]
        public void When__ANumericValueIsNotANumber__Must_Reject(string flag)
        {
            AssertRejected("--stdin", flag, "quite a few");
        }

        [TestCase("ZZ")]
        [TestCase("")]
        [TestCase("100")]
        [TestCase("0x5A")]
        public void When__TheXorValueIsNotAHexByte__Must_Reject(string value)
        {
            AssertRejected("--xor", value);
        }

        [Test]
        public void When__BothSourcesAreGiven__Must_Reject()
        {
            AssertRejected("--stdin", "--file", TestPayload.SourceFile.FullName);
        }

        [TestCase("--output-chunk-delay")]
        [TestCase("--keep-stdout-open")]
        [TestCase("--linger")]
        public void When__AWaitIsBelow_MinusOne__Must_Reject(string flag)
        {
            AssertRejected("--stdin", flag, "-2");
        }

        [TestCase("0")]
        [TestCase("-1")]
        public void When__OutputChunkSizeIs_ZeroOrNegative__Must_Reject(string value)
        {
            AssertRejected("--output-chunk-size", value);
        }

        [TestCase("0")]
        [TestCase("-1")]
        public void When__StopAfterChunksIs_ZeroOrNegative__Must_Reject(string value)
        {
            AssertRejected("--stop-after-chunks", value);
        }

        [Test]
        public void When__TheSourceFileDoesNotExist__Must_Reject()
        {
            var path = Path.Combine(Path.GetTempPath(), $"fakecmdline-no-such-file-{Guid.NewGuid():N}.bin");

            AssertRejected("--file", path);
        }

        [Test]
        public void When__KeepStdoutOpenIsGiven_WithoutASource__Must_Reject()
        {
            AssertRejected("--keep-stdout-open", "500");
        }

        [Test]
        public void When__AFlagIsRepeated__Must_Reject()
        {
            AssertRejected("--exit-code", "1", "--exit-code", "2");
        }

        [Test]
        public void When__StdinIsRepeated__Must_Reject()
        {
            AssertRejected("--stdin", "--stdin");
        }

        static void AssertRejected(params string[] arguments)
        {
            Assert.IsFalse(Arguments.TryParse(arguments, out var result), "The arguments have to be rejected");
            Assert.IsNull(result, "A rejected parse must not hand out arguments");
        }
    }
}
