using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Andy.FakeDecoder
{
    /// <summary>
    /// The bytes the fake program is fed with.
    /// The payload spans every possible byte value on purpose - including 0x00, 0x0A, 0x0D and 0x1A -
    /// so that any test asserting on content also proves that binary data survives the round trip.
    /// </summary>
    [SetUpFixture]
    public class TestPayload
    {
        public static byte[] Bytes { get; } = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();

        public static FileInfo SourceFile { get; private set; }
        public static FileInfo EmptySourceFile { get; private set; }

        /// <summary>
        /// A source file that certainly isn't there: the name is unique to the run and nothing ever writes it.
        /// </summary>
        public static FileInfo MissingSourceFile { get; } = new FileInfo(
            Path.Combine(Path.GetTempPath(), $"fakedecoder-no-such-file-{Guid.NewGuid():N}.bin"));

        static DirectoryInfo directory;

        [OneTimeSetUp]
        public void Setup()
        {
            directory = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"fakedecoder-tests-{Guid.NewGuid():N}"));

            SourceFile = Write("payload.bin", Bytes);
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
