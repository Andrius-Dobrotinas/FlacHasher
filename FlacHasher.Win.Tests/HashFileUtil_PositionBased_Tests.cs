using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Win
{
    public class HashFileUtil_PositionBased_Tests
    {
        [Test]
        public void When_NumberOf_Files_IsTheSameAs_NumberOf_ExpectedHashes__Should_Return_AllHashes_And_No_MissingFiles()
        {
            Test_When_NoFilesMissing_Should_Return_ExpectedItems(new []
                {
                    "659e7ccaa5cb9d72bb508a382fd17fdf84e0eab412d9341fd10c14edd8722277",
                    "5ea29a4b4ed755218a8fc5af7b026caaa4d98582ca21554de1e8f0c18d5a69a1"
                },
                new FileInfo[]
                {
                    new FileInfo("Nirvana_1990-08-17_SBD1_t01.flac"),
                    new FileInfo("Nirvana_1990-08-17_SBD1_t02.flac")
                });
        }

        [Test]
        public void When_MoreFiles_Than_ExpectedHashes__Should_Return_AllHashes_And_No_MissingFiles()
        {
            Test_When_NoFilesMissing_Should_Return_ExpectedItems(new[]
                {
                    "659e7ccaa5cb9d72bb508a382fd17fdf84e0eab412d9341fd10c14edd8722277",
                    "5ea29a4b4ed755218a8fc5af7b026caaa4d98582ca21554de1e8f0c18d5a69a1"
                },
                new FileInfo[]
                {
                    new FileInfo("Nirvana_1990-08-17_SBD1_t01.flac"),
                    new FileInfo("Nirvana_1990-08-17_SBD1_t02.flac"),
                    new FileInfo("Nirvana_1990-08-17_SBD1_t03.flac")
                });
        }

        private void Test_When_NoFilesMissing_Should_Return_ExpectedItems(IList<string> input_hashes, IList<FileInfo> input_files)
        {
            IList<KeyValuePair<string, string>> expected_Items = input_hashes
                .Select(hash => new KeyValuePair<string, string>("", hash))
                .ToArray();

            (IList<KeyValuePair<FileInfo, string>> result_ExpectedItems,
            IList<KeyValuePair<FileInfo, string>> result_MissingItems) = HashFileUtil.MatchFilesToHashesPositionBased(expected_Items, input_files);

            Assert.IsEmpty(result_MissingItems, "Missing files/hashes");

            Assert.IsNotEmpty(result_ExpectedItems, "Expected files/hashes");

            Verify_Result_ContainsExpected(expected_Items, input_files, expected_Items.Count, result_ExpectedItems);
        }

        [TestCaseSource(nameof(GetCases_LessFilesThanExpected))]
        public void When_LessFiles_Than_ExpectedHashes__Should_Return_TheNumberOfFirstItems_ThatThereAreFiles(IList<string> input_hashes, IList<FileInfo> input_files)
        {
            IList<KeyValuePair<string, string>> input_expectedHashes = input_hashes
                .Select(hash => new KeyValuePair<string, string>("", hash))
                .ToArray();

            (IList<KeyValuePair<FileInfo, string>> result_ExpectedItems,
            IList<KeyValuePair<FileInfo, string>> _) = HashFileUtil.MatchFilesToHashesPositionBased(input_expectedHashes, input_files);

            Verify_Result_ContainsExpected(input_expectedHashes, input_files, input_files.Count, result_ExpectedItems);
        }

        [TestCaseSource(nameof(GetCases_LessFilesThanExpected))]
        public void When_LessFiles_Than_ExpectedHashes__Should_Return_MissingItems(IList<string> hashes, IList<FileInfo> files)
        {
            IList<KeyValuePair<string, string>> expectedHashes = hashes
                .Select(hash => new KeyValuePair<string, string>("", hash))
                .ToArray();

            (IList<KeyValuePair<FileInfo, string>> _,
            IList<KeyValuePair<FileInfo, string>> result_MissingItems) = HashFileUtil.MatchFilesToHashesPositionBased(expectedHashes, files);

            Assert.IsNotEmpty(result_MissingItems);

            var diffCount = expectedHashes.Count - files.Count;

            for (int i = 0; i < diffCount; i++)
            {
                Assert.IsFalse(result_MissingItems.Count - 1 < i, "The missing-result should not contain more items than there are missing files");

                var result = result_MissingItems[diffCount - 1 + i];
                Assert.AreEqual(result_MissingItems[i].Value, result.Value, "Hash values");
                Assert.AreEqual(HashFileUtil.MissingFileKey, result.Key.Name, "File NAME has to match the pre-defined 'no-file' name");
            }
        }

        private static IEnumerable<TestCaseData> GetCases_LessFilesThanExpected()
        {
            yield return new TestCaseData(
                new string[]
                {
                    "659e7ccaa5cb9d72bb508a382fd17fdf84e0eab412d9341fd10c14edd8722277",
                    "789e7ccaa5cb9d72bb508a382fd17fdf84e0eab412d9341fd10c14edd8711188",
                    "5ea29a4b4ed755218a8fc5af7b026caaa4d98582ca21554de1e8f0c18d5a69a1"
                },
                new FileInfo[]
                {
                    new FileInfo("Nirvana_1990-08-17_SBD1_t01.flac"),
                    new FileInfo("Nirvana_1990-08-17_SBD1_t02.flac")
                });

            yield return new TestCaseData(
                new string[]
                {
                    "659e7ccaa5cb9d72bb508a382fd17fdf84e0eab412d9341fd10c14edd8722277",
                },
                new FileInfo[0]);
        }

        private void Verify_Result_ContainsExpected(
            IList<KeyValuePair<string, string>> input_expectedHashes,
            IList<FileInfo> input_files,
            int expected_ResultCount,
            IList<KeyValuePair<FileInfo, string>> result_ExpectedItems)
        {
            for (int i = 0; i < expected_ResultCount; i++)
            {
                Assert.IsFalse(result_ExpectedItems.Count - 1 < i, "The result should not contain more items than there are in the source");
                Assert.AreEqual(input_files[i], result_ExpectedItems[i].Key, $"The file should be included at position {i}: {input_files[i].FullName}");
                Assert.AreEqual(input_expectedHashes[i].Value, result_ExpectedItems[i].Value, $"The hash should be included at position {i}");
            }
        }
    }
}