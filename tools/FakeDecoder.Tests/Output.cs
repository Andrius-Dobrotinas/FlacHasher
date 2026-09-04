using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Andy.FakeDecoder
{
    public class Output
    {
        [Test]
        public async Task When__ReadingFromAFile__Must_Write_ItsBytes_ToStdout_Unaltered()
        {
            var result = await App.Run("--file", TestPayload.SourceFile.FullName);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(TestPayload.Bytes, result.StdOut);
            });
        }

        [Test]
        public async Task When__ReadingFromStdin__Must_Write_ItsBytes_ToStdout_Unaltered()
        {
            var result = await App.Run(TestPayload.Bytes, "--stdin");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(TestPayload.Bytes, result.StdOut);
            });
        }

        [TestCase(false, TestName = "When__XorIsGiven__Must_Write_TheTransformedBytes(From a file)")]
        [TestCase(true, TestName = "When__XorIsGiven__Must_Write_TheTransformedBytes(From stdin)")]
        public async Task When__XorIsGiven__Must_Write_TheTransformedBytes(bool fromStdin)
        {
            const byte mask = 0x5A;
            var expected = TestPayload.Bytes.Select(x => (byte)(x ^ mask)).ToArray();

            var result = fromStdin
                ? await App.Run(TestPayload.Bytes, "--stdin", "--xor", "5A")
                : await App.Run("--file", TestPayload.SourceFile.FullName, "--xor", "5A");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(expected, result.StdOut);
            });
        }

        [TestCase(false, TestName = "When__ExpandIsGiven__Must_Write_EveryByte_ThatManyTimesOver(From a file)")]
        [TestCase(true, TestName = "When__ExpandIsGiven__Must_Write_EveryByte_ThatManyTimesOver(From stdin)")]
        public async Task When__ExpandIsGiven__Must_Write_EveryByte_ThatManyTimesOver(bool fromStdin)
        {
            const int factor = 3;
            var expected = TestPayload.Bytes.SelectMany(x => Enumerable.Repeat(x, factor)).ToArray();

            var result = fromStdin
                ? await App.Run(TestPayload.Bytes, "--stdin", "--expand", "3")
                : await App.Run("--file", TestPayload.SourceFile.FullName, "--expand", "3");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(expected, result.StdOut);
            });
        }

        /// <summary>
        /// The two transformations are independent, and either one silently overwriting the other's work would show up here.
        /// </summary>
        [Test]
        public async Task When__ExpandAndXor_AreBothGiven__Must_Apply_BothOfThem()
        {
            const byte mask = 0x5A;
            const int factor = 2;
            var expected = TestPayload.Bytes.SelectMany(x => Enumerable.Repeat((byte)(x ^ mask), factor)).ToArray();

            var result = await App.Run("--file", TestPayload.SourceFile.FullName, "--expand", "2", "--xor", "5A");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(expected, result.StdOut);
            });
        }

        /// <summary>
        /// Held against the same run without expansion, since the read size is the one thing expansion has to leave alone.
        /// </summary>
        [Test]
        public async Task When__ExpandIsGiven__Must_Not_Change_HowMuchIsRead_PerRead()
        {
            const string progressMessage = "WRITE-WENT-OUT";
            const int readChunkSize = 10;
            const int factor = 3;

            var unexpanded = await App.Run(
                "--file", TestPayload.SourceFile.FullName,
                "--read-chunk-size", readChunkSize.ToString(),
                "--progress-message", progressMessage);

            var expanded = await App.Run(
                "--file", TestPayload.SourceFile.FullName,
                "--expand", factor.ToString(),
                "--read-chunk-size", readChunkSize.ToString(),
                "--progress-message", progressMessage);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, expanded.ExitCode, expanded.StdErr);
                Assert.AreEqual(
                    CountOccurrences(unexpanded.StdErr, progressMessage),
                    CountOccurrences(expanded.StdErr, progressMessage),
                    "reads");
                Assert.AreEqual(unexpanded.StdOut.Length * factor, expanded.StdOut.Length, "bytes written");
            });
        }

        static int CountOccurrences(string text, string value)
        {
            return text.Split(value).Length - 1;
        }

        /// <summary>
        /// 256 isn't a multiple of 10, so the final read is a short 6-byte one: bytes dropped there would show up here.
        /// </summary>
        [Test]
        public async Task When__TheFinalRead_IsPartial_WhileExpanding__Must_Still_Write_EveryByte()
        {
            const int factor = 3;
            var expected = TestPayload.Bytes.SelectMany(x => Enumerable.Repeat(x, factor)).ToArray();

            var result = await App.Run(
                "--file", TestPayload.SourceFile.FullName,
                "--expand", factor.ToString(),
                "--read-chunk-size", "10");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(expected, result.StdOut);
            });
        }

        /// <summary>
        /// The smallest possible read, one byte at a time, still has to be expanded correctly on the way out.
        /// </summary>
        [Test]
        public async Task When__TheReadChunkSizeIsOne__Must_Write_TheSource_OneByteAtATime_WhileExpanding()
        {
            const int factor = 3;
            var expected = TestPayload.Bytes.SelectMany(x => Enumerable.Repeat(x, factor)).ToArray();

            var result = await App.Run(
                "--file", TestPayload.SourceFile.FullName,
                "--expand", factor.ToString(),
                "--read-chunk-size", "1");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(expected, result.StdOut);
            });
        }

        [Test]
        public async Task When__FinishAfterReadsIsReached_WhileExpanding__Must_Read_ExactlyThatManyTimes()
        {
            const int readChunkSize = 10;
            const int factor = 2;
            const int reads = 3;
            var expected = TestPayload.Bytes
                .Take(reads * readChunkSize)
                .SelectMany(x => Enumerable.Repeat(x, factor))
                .ToArray();

            var result = await App.Run(
                "--file", TestPayload.SourceFile.FullName,
                "--expand", factor.ToString(),
                "--read-chunk-size", readChunkSize.ToString(),
                "--finish-after-reads", reads.ToString());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(expected, result.StdOut);
            });
        }

        /// <summary>
        /// 7 doesn't divide 256, so a dropped remainder would show up as missing tail bytes.
        /// </summary>
        [Test]
        public async Task When__TheReadChunkSizeDoesNotDivideTheSource__Must_Still_Write_EveryByte()
        {
            var result = await App.Run("--file", TestPayload.SourceFile.FullName, "--read-chunk-size", "7");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(TestPayload.Bytes, result.StdOut);
            });
        }

        [Test]
        public async Task When__TheSourceFileIsEmpty__Must_Write_Nothing_And_Exit_Zero()
        {
            var result = await App.Run("--file", TestPayload.EmptySourceFile.FullName);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.IsEmpty(result.StdOut);
            });
        }

        [Test]
        public async Task When__StdinIsEmpty__Must_Write_Nothing_And_Exit_Zero()
        {
            var result = await App.Run(Array.Empty<byte>(), "--stdin");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.IsEmpty(result.StdOut);
            });
        }

        [Test]
        public async Task When__FinishAfterReadsIsReached__Must_Read_ExactlyThatManyTimes()
        {
            var result = await App.Run(
                "--file", TestPayload.SourceFile.FullName,
                "--read-chunk-size", "10",
                "--finish-after-reads", "3");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(TestPayload.Bytes.Take(30).ToArray(), result.StdOut);
            });
        }
    }
}
