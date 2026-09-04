using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FakeDecoder
{
    public class Parsing
    {
        static readonly string[] allFlags =
        {
            "--file",
            "--stdin",
            "--xor",
            "--expand",
            "--read-chunk-size",
            "--output-chunk-delay",
            "--finish-after-reads",
            "--progress-message",
            "--success-message",
            "--error-message",
            "--keep-stdout-open",
            "--linger",
            "--exit-code"
        };

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
        [TestCase("00", 0x00)]
        [TestCase("FF", 0xFF)]
        public void When__XorIsGiven__Must_ParseIt_AsAHexByte(string value, byte expected)
        {
            var parsed = Parse("--xor", value);

            Assert.AreEqual(expected, parsed.Xor);
        }

        [Test]
        public void When__ExpandIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--expand", "3");

            Assert.AreEqual(3, parsed.Expand);
        }

        [Test]
        public void When__ReadChunkSizeIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--read-chunk-size", "128");

            Assert.AreEqual(128, parsed.ReadChunkSize);
        }

        [Test]
        public void When__OutputChunkDelayIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--output-chunk-delay", "250");

            Assert.AreEqual(250, parsed.OutputChunkDelayMs);
        }

        [Test]
        public void When__FinishAfterReadsIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--finish-after-reads", "3");

            Assert.AreEqual(3, parsed.FinishAfterReads);
        }

        [Test]
        public void When__ProgressMessageIsGiven__Must_ParseIt()
        {
            var parsed = Parse("--progress-message", "a chunk went out");

            Assert.AreEqual("a chunk went out", parsed.ProgressMessage);
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
                Assert.IsNull(parsed.OutputChunkDelayMs, "chunk delay");
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
                Assert.AreEqual(20, parsed.OutputChunkDelayMs, "chunk delay");
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

        [Test]
        public void When__TheBufferIsExactly_TheMaximum__Must_Accept_It()
        {
            var parsed = Parse("--read-chunk-size", (Arguments.MaxBufferBytes / 2).ToString(), "--expand", "2");

            Assert.AreEqual(Arguments.MaxBufferBytes / 2, parsed.ReadChunkSize);
        }

        [Test]
        public void When__ReadChunkSizeIs_One__Must_Accept_It()
        {
            var parsed = Parse("--read-chunk-size", "1");

            Assert.AreEqual(1, parsed.ReadChunkSize);
        }

        [Test]
        public void When__FinishAfterReadsIs_One__Must_Accept_It()
        {
            var parsed = Parse("--finish-after-reads", "1");

            Assert.AreEqual(1, parsed.FinishAfterReads);
        }

        /// <summary>
        /// Driven off the flag list so that the help can't quietly rot as flags get added.
        /// </summary>
        [TestCaseSource(nameof(allFlags))]
        public void When__UsageText__Must_Document_EveryFlag(string flag)
        {
            Assert.That(Arguments.UsageText, Does.Contain(flag));
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
                new[] { "--output-chunk-delay", "20" },
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
            yield return new TestCaseData("--output-chunk-delay", (Func<Arguments, int?>)(x => x.OutputChunkDelayMs))
                .SetName("{m}(--output-chunk-delay)");

            yield return new TestCaseData("--keep-stdout-open", (Func<Arguments, int?>)(x => x.KeepStdoutOpenMs))
                .SetName("{m}(--keep-stdout-open)");

            yield return new TestCaseData("--linger", (Func<Arguments, int?>)(x => x.LingerMs))
                .SetName("{m}(--linger)");
        }
    }
}
