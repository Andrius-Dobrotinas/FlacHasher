using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Verification
{
    public class HashVerifier_Tests
    {
        [TestCase("01ABFF", new byte[] { 0x01, 0xAB, 0xFF })]
        [TestCase("01abff", new byte[] { 0x01, 0xAB, 0xFF })]
        public void When_HashesAreEqual__Must_ReturnTrue(string expectedHash, byte[] actualHash)
        {
            var file = new FileInfo(@"c:\temp\file1.flac");
            var expectedHashes = new Dictionary<FileInfo, string>
            {
                { file, expectedHash }
            };

            var verifier = new HashVerifier();

            var result = verifier.DoesMatch(expectedHashes, file, actualHash);

            Assert.IsTrue(result);
        }

        [TestCase("01ABFF", new byte[] { 0x01, 0xAB, 0x00 })]
        public void When_HashesAreDifferent__Must_ReturnFalse(string expectedHash, byte[] actualHash)
        {
            var file = new FileInfo(@"c:\temp\file1.flac");
            var expectedHashes = new Dictionary<FileInfo, string>
            {
                { file, expectedHash }
            };

            var verifier = new HashVerifier();

            var result = verifier.DoesMatch(expectedHashes, file, actualHash);

            Assert.IsFalse(result);
        }

        [Test]
        public void When_FileNotInDictionary__Must_Throw_KeyNotFoundException()
        {
            var fileInDict = new FileInfo(@"c:\temp\file1.flac");
            var fileNotInDict = new FileInfo(@"c:\temp\file2.flac");
            var expectedHashes = new Dictionary<FileInfo, string>
            {
                { fileInDict, "01ABFF" }
            };

            var verifier = new HashVerifier();

            Assert.Throws<KeyNotFoundException>(
                () => verifier.DoesMatch(expectedHashes, fileNotInDict, new byte[] { 0x01, 0xAB, 0xFF }));
        }

        [Test]
        public void When_ExpectedHashIsNotValidHex__Must_Throw_FormatException()
        {
            var file = new FileInfo(@"c:\temp\file1.flac");
            var expectedHashes = new Dictionary<FileInfo, string>
            {
                { file, "not-a-hex-string" }
            };

            var verifier = new HashVerifier();
            var actualHash = new byte[] { 0x01, 0x02 };

            Assert.Throws<FormatException>(
                () => verifier.DoesMatch(expectedHashes, file, actualHash));
        }
    }
}
