using NUnit.Framework;
using System;
using System.IO;

namespace Andy.FakeDecoder
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
        [TestCase("--read-chunk-size")]
        [TestCase("--progress-message")]
        [TestCase("--linger")]
        [TestCase("--exit-code")]
        public void When__AFlagIsMissingItsValue__Must_Reject(string flag)
        {
            AssertRejected(flag);
        }

        [TestCase("--read-chunk-size")]
        [TestCase("--output-chunk-delay")]
        [TestCase("--finish-after-reads")]
        [TestCase("--expand")]
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
        public void When__ReadChunkSizeIs_ZeroOrNegative__Must_Reject(string value)
        {
            AssertRejected("--read-chunk-size", value);
        }

        [TestCase("0")]
        [TestCase("-1")]
        public void When__FinishAfterReadsIs_ZeroOrNegative__Must_Reject(string value)
        {
            AssertRejected("--finish-after-reads", value);
        }

        [TestCase("1")]
        [TestCase("0")]
        [TestCase("-1")]
        public void When__ExpandIsBelow_Two__Must_Reject(string value)
        {
            AssertRejected("--expand", value);
        }

        [Test]
        public void When__TheSourceFileDoesNotExist__Must_Reject()
        {
            var path = Path.Combine(Path.GetTempPath(), $"fakedecoder-no-such-file-{Guid.NewGuid():N}.bin");

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

        /// <summary>
        /// Allocating what these ask for throws, and an unhandled exception would cost the run the exit code it was given.
        /// </summary>
        [TestCase("--read-chunk-size", TestName = "{m}(In one flag)")]
        [TestCase("--expand", TestName = "{m}(Multiplied by the default chunk size)")]
        public void When__TheBufferWouldExceed_TheMaximum__Must_Reject(string flag)
        {
            AssertRejected(flag, (Arguments.MaxBufferBytes + 1).ToString());
        }

        [Test]
        public void When__TheChunkSizeAndExpansion_Multiply_PastTheMaximum__Must_Reject()
        {
            AssertRejected("--read-chunk-size", (Arguments.MaxBufferBytes / 2).ToString(), "--expand", "3");
        }

        static void AssertRejected(params string[] arguments)
        {
            Assert.IsFalse(Arguments.TryParse(arguments, out var result), "The arguments have to be rejected");
            Assert.IsNull(result, "A rejected parse must not hand out arguments");
        }
    }
}
