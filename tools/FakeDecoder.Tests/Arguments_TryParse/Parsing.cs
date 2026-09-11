using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Andy.FakeDecoder
{
    public class Parsing
    {
        [Test]
        public void When__FileIsGiven__Must_ParseIt_AsTheSourceFile()
        {
            var parsed = Parse("--file", TestPayload.SourceFile.FullName);

            Assert.AreEqual(TestPayload.SourceFile.FullName, parsed.SourceFile);
        }

        [Test]
        public void When__StdinIsGiven__Must_ParseIt_AsTheSource()
        {
            var parsed = Parse("--stdin");

            Assert.IsTrue(parsed.UseStdin);
        }

        [TestCase("5A", 0x5A)]
        [TestCase("5a", 0x5A)]
        [TestCase("5", 0x05)]
        [TestCase("00", 0x00)]
        [TestCase("FF", 0xFF)]
        public void When__XorIsGiven__Must_ParseIt_AsAHexByte(string value, byte expected)
        {
            var parsed = Parse("--stdin", "--xor", value);

            Assert.AreEqual(expected, parsed.Xor);
        }

        [Test]
        public void When__ExpandIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--stdin", "--expand", "3");

            Assert.AreEqual(3, parsed.Expand);
        }

        [Test]
        public void When__ReadChunkSizeIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--stdin", "--read-chunk-size", "128");

            Assert.AreEqual(128, parsed.ReadChunkSize);
        }

        [Test]
        public void When__WriteDelayIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--stdin", "--write-delay", "250");

            Assert.AreEqual(250, parsed.WriteDelayMs);
        }

        [Test]
        public void When__FinishAfterReadsIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--stdin", "--finish-after-reads", "3");

            Assert.AreEqual(3, parsed.FinishAfterReads);
        }

        [Test]
        public void When__ProgressMessageIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--stdin", "--progress-message", "a write went out");

            Assert.AreEqual("a write went out", parsed.ProgressMessage);
        }

        [Test]
        public void When__SuccessMessageIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--success-message", "all good");

            Assert.AreEqual("all good", parsed.SuccessMessage);
        }

        [Test]
        public void When__ErrorMessageIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--error-message", "it went wrong");

            Assert.AreEqual("it went wrong", parsed.ErrorMessage);
        }

        [Test]
        public void When__KeepStdoutOpenIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--stdin", "--keep-stdout-open", "500");

            Assert.AreEqual(500, parsed.KeepStdoutOpenMs);
        }

        [Test]
        public void When__LingerIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--linger", "700");

            Assert.AreEqual(700, parsed.LingerMs);
        }

        [TestCase("0", 0)]
        [TestCase("3", 3)]
        [TestCase("42", 42)]
        [TestCase("-1", -1)]
        public void When__ExitCodeIsGiven__Must_ParseIt(string value, int expected)
        {
            var parsed = Parse("--exit-code", value);

            Assert.AreEqual(expected, parsed.ExitCode);
        }

        [Test]
        public void When__NoArgumentsAreGiven__Must_Succeed_With_Defaults()
        {
            var parsed = Parse();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(Arguments.DefaultReadChunkSize, parsed.ReadChunkSize, "chunk size");
                Assert.AreEqual(0, parsed.ExitCode, "exit code");
                Assert.IsNull(parsed.SourceFile, "source file");
                Assert.IsFalse(parsed.UseStdin, "stdin");
                Assert.IsNull(parsed.Xor, "xor");
                Assert.IsNull(parsed.Expand, "expand");
                Assert.IsNull(parsed.WriteDelayMs, "write delay");
                Assert.IsNull(parsed.FinishAfterReads, "finish after reads");
                Assert.IsNull(parsed.ProgressMessage, "progress message");
                Assert.IsNull(parsed.SuccessMessage, "success message");
                Assert.IsNull(parsed.ErrorMessage, "error message");
                Assert.IsNull(parsed.KeepStdoutOpenMs, "keep stdout open");
                Assert.IsNull(parsed.LingerMs, "linger");
            });
        }

        [TestCaseSource(nameof(GetFlagOrderings))]
        public void When__FlagsAreGiven_InAnyOrder__Must_ParseThemAll(string[] arguments)
        {
            var parsed = Parse(arguments);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(parsed.UseStdin, "stdin");
                Assert.AreEqual(0x5A, parsed.Xor, "xor");
                Assert.AreEqual(2, parsed.Expand, "expand");
                Assert.AreEqual(16, parsed.ReadChunkSize, "chunk size");
                Assert.AreEqual(20, parsed.WriteDelayMs, "write delay");
                Assert.AreEqual(2, parsed.FinishAfterReads, "finish after reads");
                Assert.AreEqual("progress", parsed.ProgressMessage, "progress message");
                Assert.AreEqual("success", parsed.SuccessMessage, "success message");
                Assert.AreEqual("error", parsed.ErrorMessage, "error message");
                Assert.AreEqual(30, parsed.KeepStdoutOpenMs, "keep stdout open");
                Assert.AreEqual(40, parsed.LingerMs, "linger");
                Assert.AreEqual(7, parsed.ExitCode, "exit code");
            });
        }

        [TestCaseSource(nameof(GetWaitFlags))]
        public void When__AWaitIs_MinusOne__Must_Accept_It(string flag, Func<Arguments, int?> getValue)
        {
            var parsed = Parse("--stdin", flag, "-1");

            Assert.AreEqual(-1, getValue(parsed));
        }

        /// <summary>
        /// The read buffer counts towards the cap as well as the expanded one, so a chunk of a quarter the maximum
        /// expanded three times over sits exactly on it: four buffers' worth in all.
        /// </summary>
        [Test]
        public void When__TheBuffersAreExactly_TheMaximum__Must_Accept_It()
        {
            var parsed = Parse("--stdin", "--read-chunk-size", (Arguments.MaxBufferBytes / 4).ToString(), "--expand", "3");

            Assert.AreEqual(Arguments.MaxBufferBytes / 4, parsed.ReadChunkSize);
        }

        /// <summary>
        /// Without an expansion there's no second buffer, so the whole maximum is the read chunk's to use.
        /// </summary>
        [Test]
        public void When__TheReadBufferAlone_IsExactly_TheMaximum__Must_Accept_It()
        {
            var parsed = Parse("--stdin", "--read-chunk-size", Arguments.MaxBufferBytes.ToString());

            Assert.AreEqual(Arguments.MaxBufferBytes, parsed.ReadChunkSize);
        }

        [Test]
        public void When__ReadChunkSizeIs_One__Must_Accept_It()
        {
            var parsed = Parse("--stdin", "--read-chunk-size", "1");

            Assert.AreEqual(1, parsed.ReadChunkSize);
        }

        [Test]
        public void When__FinishAfterReadsIs_One__Must_Accept_It()
        {
            var parsed = Parse("--stdin", "--finish-after-reads", "1");

            Assert.AreEqual(1, parsed.FinishAfterReads);
        }

        /// <summary>
        /// The other side of the source requirement: these four act on the run itself rather than on bytes,
        /// so a sourceless run - one that starts, says it has nothing to do, and takes its time going - still needs them.
        /// </summary>
        [TestCase("--linger", "100", TestName = "{m}(--linger)")]
        [TestCase("--success-message", "done", TestName = "{m}(--success-message)")]
        [TestCase("--error-message", "oops", TestName = "{m}(--error-message)")]
        [TestCase("--exit-code", "3", TestName = "{m}(--exit-code)")]
        public void When__AFlagNeedingNoSource_IsGivenWithoutOne__Must_Accept_It(string flag, string value)
        {
            Assert.IsTrue(Arguments.TryParse(new[] { flag, value }, out _));
        }

        /// <summary>
        /// Driven off the flag list so that the help can't quietly rot as flags get added.
        /// </summary>
        [TestCaseSource(typeof(Flags), nameof(Flags.All))]
        public void When__UsageText__Must_Document_EveryFlag(string flag)
        {
            Assert.That(Arguments.UsageText, Does.Contain(flag));
        }

        /// <summary>
        /// And the other way about, so that the two can't drift apart from the far end either: a flag added to the
        /// help but not to the list would leave the list no longer the flag list, and every test driven off it -
        /// the missing-value cases among them - would silently stop covering that flag.
        /// The help sits next to the parsing it describes, which makes it the closest thing to the real flag set
        /// that a test can get at.
        /// </summary>
        [Test]
        public void When__TheFlagList__Must_Hold_EveryFlag_TheUsageTextMentions()
        {
            var documented = Regex.Matches(Arguments.UsageText, "--[a-z][a-z-]*")
                .Select(x => x.Value)
                .Distinct();

            Assert.That(documented, Is.EquivalentTo(Flags.All));
        }

        static Arguments Parse(params string[] arguments)
        {
            Assert.IsTrue(Arguments.TryParse(arguments, out var result), "The arguments have to be accepted");

            return result;
        }

        static IEnumerable<TestCaseData> GetFlagOrderings()
        {
            var everyFlag = new[]
            {
                new[] { "--stdin" },
                new[] { "--xor", "5A" },
                new[] { "--expand", "2" },
                new[] { "--read-chunk-size", "16" },
                new[] { "--write-delay", "20" },
                new[] { "--finish-after-reads", "2" },
                new[] { "--progress-message", "progress" },
                new[] { "--success-message", "success" },
                new[] { "--error-message", "error" },
                new[] { "--keep-stdout-open", "30" },
                new[] { "--linger", "40" },
                new[] { "--exit-code", "7" }
            };

            yield return new TestCaseData(new object[] { everyFlag.SelectMany(x => x).ToArray() })
                .SetName("{m}(In the documented order)");

            yield return new TestCaseData(new object[] { everyFlag.Reverse().SelectMany(x => x).ToArray() })
                .SetName("{m}(In the reverse order)");
        }

        static IEnumerable<TestCaseData> GetWaitFlags()
        {
            yield return new TestCaseData("--write-delay", (Func<Arguments, int?>)(x => x.WriteDelayMs))
                .SetName("{m}(--write-delay)");

            yield return new TestCaseData("--keep-stdout-open", (Func<Arguments, int?>)(x => x.KeepStdoutOpenMs))
                .SetName("{m}(--keep-stdout-open)");

            yield return new TestCaseData("--linger", (Func<Arguments, int?>)(x => x.LingerMs))
                .SetName("{m}(--linger)");
        }
    }
}
