using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// The bytes the fake decoder is fed with.
    /// The payload spans every possible byte value on purpose - including 0x00, 0x0A, 0x0D and 0x1A -
    /// so that any test asserting on content also proves that binary data survives the round trip.
    /// </summary>
    [SetUpFixture]
    public class TestPayload
    {
        /// <summary>
        /// Comfortably past the ~64 KiB a pipe will hold, so that a write to it has to block rather than
        /// disappear into the buffer. Anything relying on back-pressure is meaningless with a smaller payload.
        /// </summary>
        public const int LargeSize = 256 * 1024;

        public static byte[] Bytes { get; } = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();

        public static byte[] LargeBytes { get; } = Enumerable.Range(0, LargeSize).Select(x => (byte)(x % 256)).ToArray();

        public static FileInfo SourceFile { get; private set; }
        public static FileInfo LargeSourceFile { get; private set; }
        public static FileInfo EmptySourceFile { get; private set; }

        static DirectoryInfo directory;

        /// <summary>
        /// What the decoder makes of <paramref name="source"/>: every byte repeated <paramref name="expand"/> times,
        /// each XORed on its way out. Per-byte and identical across duplicates, so expanding and XORing commute.
        /// </summary>
        public static byte[] Expected(byte[] source, int expand = 1, byte xor = 0)
        {
            return source.SelectMany(x => Enumerable.Repeat((byte)(x ^ xor), expand)).ToArray();
        }

        [OneTimeSetUp]
        public void Setup()
        {
            directory = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"processrunner-tests-{Guid.NewGuid():N}"));

            SourceFile = Write("payload.bin", Bytes);
            LargeSourceFile = Write("payload-large.bin", LargeBytes);
            EmptySourceFile = Write("empty.bin", Array.Empty<byte>());
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            directory?.Delete(recursive: true);
        }

        static FileInfo Write(string fileName, byte[] content)
        {
            var path = Path.Combine(directory.FullName, fileName);
            File.WriteAllBytes(path, content);

            return new FileInfo(path);
        }
    }
}
