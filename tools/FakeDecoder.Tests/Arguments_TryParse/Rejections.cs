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

        [TestCaseSource(typeof(Flags), nameof(Flags.TakingAValue))]
        public void When__AFlagIsMissingItsValue__Must_Reject(string flag)
        {
            AssertRejected(flag);
        }

        [TestCase("--read-chunk-size")]
        [TestCase("--write-delay")]
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
        [TestCase(" 5A ")]
        public void When__TheXorValueIsNotAHexByte__Must_Reject(string value)
        {
            AssertRejected("--stdin", "--xor", value);
        }

        [Test]
        public void When__BothSourcesAreGiven__Must_Reject()
        {
            AssertRejected("--stdin", "--file", TestPayload.SourceFile.FullName);
        }

        [TestCase("--write-delay")]
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
            AssertRejected("--stdin", "--read-chunk-size", value);
        }

        [TestCase("0")]
        [TestCase("-1")]
        public void When__FinishAfterReadsIs_ZeroOrNegative__Must_Reject(string value)
        {
            AssertRejected("--stdin", "--finish-after-reads", value);
        }

        [TestCase("1")]
        [TestCase("0")]
        [TestCase("-1")]
        public void When__ExpandIsBelow_Two__Must_Reject(string value)
        {
            AssertRejected("--stdin", "--expand", value);
        }

        [Test]
        public void When__TheSourceFileDoesNotExist__Must_Reject()
        {
            AssertRejected("--file", TestPayload.MissingSourceFile.FullName);
        }

        /// <summary>
        /// An easy slip for a test to make, and one the open turns down outright rather than reporting as a file that
        /// isn't there. A blank path is left out of this: it's a legal name on Unix and a malformed one on Windows,
        /// so it would be proving two different things at once.
        /// </summary>
        [Test]
        public void When__TheSourceFileIsAnEmptyPath__Must_Reject()
        {
            AssertRejected("--file", "");
        }

        /// <summary>
        /// It's there, and it still can't be read: the file is held with no sharing, which denies the open on both
        /// platforms - .NET stands FileShare up on Unix with an advisory lock.
        /// A refusal over permissions rather than a lock arrives as a different exception type on Unix, and there's no
        /// way to provoke one here - the test container runs as root, and root reads whatever it likes.
        /// </summary>
        [Test]
        public void When__TheSourceFileExists_ButCannotBeOpened__Must_Reject()
        {
            var path = Path.Combine(Path.GetTempPath(), $"fakedecoder-held-{Guid.NewGuid():N}.bin");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });

            try
            {
                using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    AssertRejected("--file", path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// A run without a source reads and writes nothing, so none of these can do what it was given for.
        /// Ignoring them would leave the run looking like the one that was asked for while doing something else.
        /// </summary>
        [TestCase("--xor", "5A")]
        [TestCase("--expand", "2")]
        [TestCase("--read-chunk-size", "10")]
        [TestCase("--write-delay", "100")]
        [TestCase("--finish-after-reads", "1")]
        [TestCase("--progress-message", "progress")]
        [TestCase("--keep-stdout-open", "500")]
        public void When__AFlagNeedingASource_IsGivenWithoutOne__Must_Reject(string flag, string value)
        {
            AssertRejected(flag, value);
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
        [TestCase("--expand", TestName = "{m}(Through an expansion of the default chunk size)")]
        public void When__TheBufferWouldExceed_TheMaximum__Must_Reject(string flag)
        {
            AssertRejected("--stdin", flag, (Arguments.MaxBufferBytes + 1).ToString());
        }

        /// <summary>
        /// The largest expansion there is. Counting the extra buffer in int would wrap this negative and wave through
        /// the very demand the cap is here to stop, at the cost of the exit code the run was given.
        /// </summary>
        [Test]
        public void When__TheExpansionIs_TheLargestInt__Must_Reject()
        {
            AssertRejected("--stdin", "--expand", int.MaxValue.ToString());
        }

        /// <summary>
        /// A byte past the boundary the accepting test sits on: it's the read buffer that tips it over, so leaving
        /// that one out of the reckoning would let this through.
        /// </summary>
        [Test]
        public void When__TheReadChunkAndItsExpansion_AddUp_PastTheMaximum__Must_Reject()
        {
            AssertRejected("--stdin", "--read-chunk-size", (Arguments.MaxBufferBytes / 4 + 1).ToString(), "--expand", "3");
        }

        /// <summary>
        /// A mistyped invocation rather than an exotic value. Left alone, the flag is swallowed as the other one's
        /// text and the run carries on with arguments the test never asked for.
        /// </summary>
        [TestCase("--progress-message", "--stdin", TestName = "{m}(A valueless flag as a message)")]
        [TestCase("--success-message", "--linger", TestName = "{m}(A flag taking a value, as a message)")]
        [TestCase("--file", "--stdin", TestName = "{m}(A valueless flag as a path)")]
        public void When__AValueIsAFlagName__Must_Reject(string flag, string value)
        {
            AssertRejected(flag, value);
        }

        static void AssertRejected(params string[] arguments)
        {
            Assert.IsFalse(Arguments.TryParse(arguments, out var result), "The arguments have to be rejected");
            Assert.IsNull(result, "A rejected parse must not hand out arguments");
        }
    }
}
