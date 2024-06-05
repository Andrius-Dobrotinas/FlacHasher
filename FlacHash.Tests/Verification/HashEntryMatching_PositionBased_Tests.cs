using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class HashEntryMatching_PositionBased_Tests
    {
        const string hash1 = "659e7-hash1";
        const string hash2 = "5ea29-hash2";
        const string hash3 = "1235e-hash3";
        const string hash4 = "6789e-hash4";
        const string hash5 = "789e7-hash5";
        const string hash6 = "878e5-hash6";

        const string path = @"d:\boots";
        const string filename1 = "Nirvana_1990-08-17_SBD1_t01.flac";
        const string filename2 = "Nirvana_1990-08-17_SBD1_t02.flac";
        const string filename3 = "Nirvana_1990-08-17_SBD1_t03.flac";
        const string filename4 = "Nirvana_1990-08-17_SBD1_t04.flac";
        const string filename5 = "Nirvana_1990-08-17_SBD1_t05.flac";
        const string filename6 = "Nirvana_1990-08-17_SBD1_t06.flac";

        [TestCaseSource(nameof(GetCases_NumberOfFilesMatchesHashes))]
        public void When_Theres_TheSameNumber_OfFiles_As_TheNumberOf_Hashes__Must_Return_AllHashes_MatchedWithFilesAtRespectiveIndexes_And_No_MissingFiles(IList<KeyValuePair<string, string>> inputHashes, IList<FileInfo> inputFiles, IList<KeyValuePair<FileInfo, string>> expected)
        {
            var (result_presentItems, result_missing) = HashEntryMatching.MatchFilesToHashesPositionBased(inputHashes, inputFiles);

            Assert.IsNotEmpty(result_presentItems);

            AssertThat.CollectionsMatchExactly(
                result_presentItems.Select(x => x.Key),
                expected.Select(x => x.Key),
                "File infos");

            AssertThat.CollectionsMatchExactly(
                result_presentItems.Select(x => x.Value),
                expected.Select(x => x.Value),
                "Hash values");

            Assert.IsEmpty(result_missing);
        }

        [TestCaseSource(nameof(GetCases_MoreFilesThanExpected))]
        public void When_Theres_MoreFiles_Than_Hashes__Must_Return_AllHashes_PairedWithTheSameNumberOfFirstFiles_And_No_MissingFiles(IList<KeyValuePair<string, string>> inputHashes, IList<FileInfo> inputFiles, IList<KeyValuePair<FileInfo, string>> expected)
        {
            var (result_presentItems, result_missing) = HashEntryMatching.MatchFilesToHashesPositionBased(inputHashes, inputFiles);

            Assert.IsNotEmpty(result_presentItems);

            AssertThat.CollectionsMatchExactly(
                result_presentItems.Select(x => x.Key),
                expected.Select(x => x.Key),
                "File infos");

            AssertThat.CollectionsMatchExactly(
                result_presentItems.Select(x => x.Value),
                expected.Select(x => x.Value),
                "Hash values");

            Assert.IsEmpty(result_missing);
        }

        [TestCaseSource(nameof(GetCases_FewerFilesThanExpected))]
        public void When_Theres_FewerFiles_Than_ExpectedHashes__Must_Return_TheSameNumberOfFirstHashes_PairedWithFiles(string description, IList<KeyValuePair<string, string>> inputHashes, IList<FileInfo> inputFiles, IList<KeyValuePair<FileInfo, string>> expected, IList<string> _)
        {
            var (result_presentItems, result_missing) = HashEntryMatching.MatchFilesToHashesPositionBased(inputHashes, inputFiles);

            Assert.IsNotEmpty(result_presentItems);

            AssertThat.CollectionsMatchExactly(
                result_presentItems.Select(x => x.Key),
                expected.Select(x => x.Key),
                "File infos");

            AssertThat.CollectionsMatchExactly(
                result_presentItems.Select(x => x.Value),
                expected.Select(x => x.Value),
                "Hash values");
        }

        [TestCaseSource(nameof(GetCases_FewerFilesThanExpected))]
        public void When_Theres_FewerFiles_Than_ExpectedHashes__Must_Return_TheExcessHashes_As_MissingItems(string description, IList<KeyValuePair<string, string>> inputHashes, IList<FileInfo> inputFiles, IList<KeyValuePair<FileInfo, string>> _, IList<string> expected)
        {
            var (result_presentItems, result_missingItems) = HashEntryMatching.MatchFilesToHashesPositionBased(inputHashes, inputFiles);

            Assert.IsNotEmpty(result_missingItems);

            Assert.Multiple(() =>
            {
                var distinctValues = result_missingItems.Select(x => x.Key.Name).Distinct();
                Assert.AreEqual(HashEntryMatching.MissingFileKey, distinctValues.First(), "Missing file info Name");
                Assert.AreEqual(1, distinctValues.Count(), "Use the same name for a missing file");
            });

            AssertThat.CollectionsMatchExactly(
                result_missingItems.Select(x => x.Value),
                expected,
                "Hash values");
        }

        [TestCaseSource(nameof(GetCases_NoInputFiles))]
        public void When_Theres_NoFiles__Must_Return_AllHashes_As_Missing(IList<KeyValuePair<string, string>> inputHashes)
        {
            var expectedHashes = inputHashes.Select(x => x.Value).ToList();

            var (result_presentItems, result_missing) = HashEntryMatching.MatchFilesToHashesPositionBased(inputHashes, Array.Empty<FileInfo>());

            Assert.IsEmpty(result_presentItems);
            Assert.IsNotEmpty(result_missing);

            Assert.Multiple(() =>
            {
                var distinctValues = result_missing.Select(x => x.Key.Name).Distinct();
                Assert.AreEqual(HashEntryMatching.MissingFileKey, distinctValues.First(), "Missing file info Name");
                Assert.AreEqual(1, distinctValues.Count(), "Use the same name for a missing file");
            });

            AssertThat.CollectionsMatchExactly(
                result_missing.Select(x => x.Value),
                expectedHashes,
                "Hash values");
        }

        private static IEnumerable<TestCaseData> GetCases_NumberOfFilesMatchesHashes()
        {
            var file1 = new FileInfo($@"{path}\{filename1}");
            var file2 = new FileInfo($@"{path}\{filename2}");
            var file3 = new FileInfo($@"{path}\{filename3}");
            var file4 = new FileInfo($@"{path}\{filename4}");
            
            yield return new TestCaseData(
                // hashes
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, hash1),
                },
                new FileInfo[]
                {
                    file1
                },
                // expected
                new KeyValuePair<FileInfo, string>[]
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                });

            yield return new TestCaseData(
                // hashes
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, hash1),
                    new KeyValuePair<string, string>(null, hash2),
                    new KeyValuePair<string, string>(null, hash3),
                    new KeyValuePair<string, string>(null, hash4)
                },
                new FileInfo[]
                {
                    file1,
                    file2,
                    file3,
                    file4,
                },
                // expected - present
                new KeyValuePair<FileInfo, string>[]
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(file3, hash3),
                    new KeyValuePair<FileInfo, string>(file4, hash4)
                });
        }

        private static IEnumerable<TestCaseData> GetCases_MoreFilesThanExpected()
        {
            var file1 = new FileInfo($@"{path}\{filename1}");
            var file2 = new FileInfo($@"{path}\{filename2}");
            var file3 = new FileInfo($@"{path}\{filename3}");
            var file4 = new FileInfo($@"{path}\{filename4}");
            var file5 = new FileInfo($@"{path}\{filename5}");

            yield return new TestCaseData(
                // hashes
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, hash1),
                    new KeyValuePair<string, string>(null, hash2)
                },
                new FileInfo[]
                {
                    file1,
                    file2,
                    file3
                },
                // expected
                new KeyValuePair<FileInfo, string>[]
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2)
                });

            yield return new TestCaseData(
                // hashes
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, hash1),
                    new KeyValuePair<string, string>(null, hash2),
                    new KeyValuePair<string, string>(null, hash3),
                },
                new FileInfo[]
                {
                    file1,
                    file2,
                    file3,
                    file4,
                    file5
                },
                // expected - present
                new KeyValuePair<FileInfo, string>[]
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(file3, hash3)
                });
        }

        private static IEnumerable<TestCaseData> GetCases_FewerFilesThanExpected()
        {
            
            var file1 = new FileInfo($@"{path}\{filename1}");
            var file2 = new FileInfo($@"{path}\{filename2}");
            var file3 = new FileInfo($@"{path}\{filename3}");

            yield return new TestCaseData(
                "Last file missing",
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, hash1),
                    new KeyValuePair<string, string>(null, hash2),
                    new KeyValuePair<string, string>(null, hash3)
                },
                new FileInfo[]
                {
                    file1, file2
                },
                // expected - present
                new KeyValuePair<FileInfo, string>[]
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                },
                // expected - missing
                new string[]
                {
                    hash3
                });

            yield return new TestCaseData(
                "More than one file missing",
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, hash1),
                    new KeyValuePair<string, string>(null, hash2),
                    new KeyValuePair<string, string>(null, hash3),
                    new KeyValuePair<string, string>(null, hash4)
                },
                new FileInfo[]
                {
                    file1, file3
                },
                // expected - present
                new KeyValuePair<FileInfo, string>[]
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file3, hash2),
                },
                // expected - missing
                new string[]
                {
                    hash3, hash4
                });
        }

        private static IEnumerable<TestCaseData> GetCases_NoInputFiles()
        {
            yield return new TestCaseData(
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, hash2)
                });

            yield return new TestCaseData(
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, hash1),
                    new KeyValuePair<string, string>(null, hash2),
                    new KeyValuePair<string, string>(null, hash3)
                });
        }
    }
}