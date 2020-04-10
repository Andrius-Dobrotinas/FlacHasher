using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash
{
    public class MultipleFileHasherTests
    {
        private MultipleFileHasher target;
        private Mock<IFileHasher> hasher;

        [SetUp]
        public void Setup()
        {
            hasher = new Mock<IFileHasher>();
            target = new MultipleFileHasher(hasher.Object);
        }

        [TestCaseSource(nameof(GetFiles))]
        public void Should_Not_ComputeHashes_UntilResultsAreEnumerated(IList<FileInfo> files)
        {
            target.ComputeHashes(files);

            hasher.Verify(
                x => x.ComputerHash(
                    It.IsAny<FileInfo>()),
                Times.Never,
                "Should not do anything before enumeration");
        }

        [TestCaseSource(nameof(GetFiles))]
        public void Should_ComputeHashes_ForEachFile_WhenResultsAreEnumerated(IList<FileInfo> files)
        {
            target.ComputeHashes(files)
                .ToArray();

            hasher.Verify(
                x => x.ComputerHash(
                    It.IsAny<FileInfo>()),
                Times.Exactly(files.Count),
                "Should invoke the hash computation function exactly the number of times there are files");

            for(int i = 0; i < files.Count; i++)
            {
                hasher.Verify(
                x => x.ComputerHash(
                    It.Is<FileInfo>(
                        arg => arg == files[i])),
                $"Should invoke the hash computation function for file under index {i} ('{files[i].FullName}')");
            }
        }

        private static IEnumerable<TestCaseData> GetFiles()
        {
            yield return new TestCaseData(
                new List<FileInfo>
                {
                    new FileInfo("path")
                });

            yield return new TestCaseData(
                new List<FileInfo>
                {
                    new FileInfo("path1"),
                    new FileInfo("path2"),
                    new FileInfo("path3"),
                });
        }
    }
}