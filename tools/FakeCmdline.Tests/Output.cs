using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Andy.FakeCmdline
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

        /// <summary>
        /// 7 doesn't divide 256, so a dropped remainder would show up as missing tail bytes.
        /// </summary>
        [Test]
        public async Task When__TheChunkSizeDoesNotDivideTheSource__Must_Still_Write_EveryByte()
        {
            var result = await App.Run("--file", TestPayload.SourceFile.FullName, "--output-chunk-size", "7");

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
        public async Task When__StopAfterChunksIsReached__Must_Write_ExactlyThatManyChunks()
        {
            var result = await App.Run(
                "--file", TestPayload.SourceFile.FullName,
                "--output-chunk-size", "10",
                "--stop-after-chunks", "3");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(TestPayload.Bytes.Take(30).ToArray(), result.StdOut);
            });
        }
    }
}
