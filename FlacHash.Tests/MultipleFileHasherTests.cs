using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            target = new MultipleFileHasher(hasher.Object, true);
        }

        MultipleFileHasher CreateTarget(bool failOnError)
        {
            return new MultipleFileHasher(hasher.Object, failOnError);
        }

        [TestCaseSource(nameof(GetFiles))]
        public void Must_Not_ComputeHashes_UntilResultsAreEnumerated(IList<FileInfo> files)
        {
            target.ComputeHashes(files);

            hasher.Verify(
                x => x.ComputerHash(
                    It.IsAny<FileInfo>()),
                Times.Never,
                "Must not do anything before enumeration");
        }

        [TestCaseSource(nameof(GetFiles))]
        public void Must_ComputeHashes_ForEachFile_WhenResultsAreEnumerated(IList<FileInfo> files)
        {
            target.ComputeHashes(files)
                .ToArray();

            hasher.Verify(
                x => x.ComputerHash(
                    It.IsAny<FileInfo>()),
                Times.Exactly(files.Count),
                "Must invoke the hash computation function for each file");

            for(int i = 0; i < files.Count; i++)
            {
                hasher.Verify(
                x => x.ComputerHash(
                    It.Is<FileInfo>(
                        arg => arg == files[i])),
                $"Must invoke the hash computation function for file. Expected for file at position {i} ('{files[i].FullName}')");
            }
        }

        [TestCaseSource(nameof(GetFilesWithResults))]
        public void Must_Return_HashComputation_Result_AlongWith_File(IList<KeyValuePair<FileInfo, byte[]>> filesWithResults)
        {
            var files = filesWithResults.Select(x => x.Key).ToArray();
            var expectedResults = filesWithResults.Select(x => x.Value);

            foreach (var (file, hash) in filesWithResults)
                hasher.Setup(
                    x => x.ComputerHash(
                        It.Is<FileInfo>(arg => arg == file)))
                    .Returns(hash);

            var results = target.ComputeHashes(files)
                .ToArray();

            AssertThat.CollectionsMatchExactly(results.Select(x => x.File), files, "Files");
            AssertThat.CollectionsMatchExactly(results.Select(x => x.Hash), expectedResults, "Hashes");
        }

        [TestCaseSource(nameof(GetFilesWithExceptions))]
        public void When_ComputationErrorsOut_And_ConfiguredToContinueOnError__Must_ProcessAllFiles_And_ReturnException_For_FailedOnes(IList<(FileInfo, byte[], Exception)> filesWithResults)
        {
            var expected = filesWithResults.Select(x => new FileHashResult { File = x.Item1, Hash = x.Item2, Exception = x.Item3 }).ToArray();
            var files = expected.Select(x => x.File).ToArray();

            foreach (var item in expected)
                hasher.Setup(
                    x => x.ComputerHash(
                        It.Is<FileInfo>(arg => arg == item.File)))
                    .Returns<FileInfo>(f => item.Hash ?? throw item.Exception);

            target = CreateTarget(true);

            var results = target.ComputeHashes(files)
                .ToArray();

            AssertThat.CollectionsMatchExactly(results.Select(x => x.File), files, "Files");
            AssertThat.CollectionsMatchExactly(results.Select(x => x.Hash), expected.Select(x => x.Hash), "Hashes");
            AssertThat.CollectionsMatchExactly(results.Select(x => x.Exception), expected.Select(x => x.Exception), toString: x => x?.Message, "Exceptions");
        }

        [TestCaseSource(nameof(GetFilesWithExceptions))]
        public void When_ComputationErrorsOut_And_ConfiguredToFailOnError__Must_Rethrow_TheException_Rightaway(IList<(FileInfo, byte[], Exception)> filesWithResults)
        { 
            var files = filesWithResults.Select(x => x.Item1).ToArray();
            var expectedException = filesWithResults.Select(x => x.Item3).First(x => x != null);

            foreach (var (file, hash, exception) in filesWithResults)
                hasher.Setup(
                    x => x.ComputerHash(
                        It.Is<FileInfo>(arg => arg == file)))
                    .Returns<FileInfo>(f => hash ?? throw exception);

            target = CreateTarget(false);

            Assert.Throws(
                expectedException.GetType(),
                () => target.ComputeHashes(files)
                            .ToArray());
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
                    new FileInfo("path3")
                });
        }

        private static IEnumerable<TestCaseData> GetFilesWithResults()
        {
            yield return new TestCaseData(
                new KeyValuePair<FileInfo, byte[]>[]
                {
                    new KeyValuePair<FileInfo, byte[]>(new FileInfo("path1"), new byte[] { 1, 2, 1, 0 } ),
                    new KeyValuePair<FileInfo, byte[]>(new FileInfo("path2"), new byte[] { 2, 1, 2, 0 } ),
                    new KeyValuePair<FileInfo, byte[]>(new FileInfo("path3"), new byte[] { 3, 2, 3, 1 } )
                });
        }

        private static IEnumerable<TestCaseData> GetFilesWithExceptions()
        {
            yield return new TestCaseData(
                new (FileInfo, byte[], Exception)[]
                {
                    (new FileInfo("path1"), new byte[] { 1, 2, 1, 0 }, null),
                    (new FileInfo("path2"), null, new Exception("error'd out")),
                    (new FileInfo("path3"), new byte[] { 3, 2, 3, 1 }, null )
                });

            yield return new TestCaseData(
                new (FileInfo, byte[], Exception)[]
                {
                    (new FileInfo("path1"), null, new ArithmeticException("outta luck!")),
                    (new FileInfo("path2"), null, new Exception("error'd out - next!")),
                    (new FileInfo("path3"), new byte[] { 3, 2, 3, 1 }, null )
                });
        }
    }
}