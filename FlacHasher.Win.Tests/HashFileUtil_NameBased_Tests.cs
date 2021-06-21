using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Win
{
    public class HashFileUtil_NameBased_Tests
    {
        [Test]
        public void When_ExpectedFilesArePresent__Should_Return_AllExpectedHashes_And_No_MissingFiles()
        {
            var fileName1 = "Nirvana_1990-08-17_SBD1_t01.flac";
            var fileName2 = "Nirvana_1990-08-17_SBD1_t02.flac";
            var file1 = new FileInfo(fileName1);
            var file2 = new FileInfo(fileName2);

            var hash1 = "659e7ccaa5cb9d72bb508a382fd17fdf84e0eab412d9341fd10c14edd8722277";
            var hash2 = "5ea29a4b4ed755218a8fc5af7b026caaa4d98582ca21554de1e8f0c18d5a69a1";

            var input_HashItems = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(fileName1, hash1),
                    new KeyValuePair<string, string>(fileName2, hash2)
                };

            var input_Files = new FileInfo[]
                {
                    file1,
                    file2
                };

            var expected_ExpectedItems = new List<KeyValuePair<FileInfo, string>>
            {
                new KeyValuePair<FileInfo, string>(file1, hash1),
                new KeyValuePair<FileInfo, string>(file2, hash2)
            };

            Test_When_NoFilesMissing_Should_Return_ExpectedItems(input_HashItems, input_Files, expected_ExpectedItems);
        }

        [Test]
        public void When_ThereAreExtraUnexpectedFiles__Should_Return_AllExpectedHashes_And_No_MissingFiles()
        {
            var fileName1 = "Nirvana_1990-08-17_SBD1_t01.flac";
            var fileName2 = "Nirvana_1990-08-17_SBD1_t02.flac";
            var file1 = new FileInfo(fileName1);
            var file2 = new FileInfo(fileName2);

            var hash1 = "659e7ccaa5cb9d72bb508a382fd17fdf84e0eab412d9341fd10c14edd8722277";
            var hash2 = "5ea29a4b4ed755218a8fc5af7b026caaa4d98582ca21554de1e8f0c18d5a69a1";

            var input_HashItems = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(fileName1, hash1),
                    new KeyValuePair<string, string>(fileName2, hash2)
                };

            var input_Files = new FileInfo[]
                {
                    file1,
                    file2,
                    new FileInfo("Nirvana_1990-08-17_SBD1_t03.flac")
                };

            var expected_ExpectedItems = new List<KeyValuePair<FileInfo, string>>
            {
                new KeyValuePair<FileInfo, string>(file1, hash1),
                new KeyValuePair<FileInfo, string>(file2, hash2)
            };

            Test_When_NoFilesMissing_Should_Return_ExpectedItems(input_HashItems, input_Files, expected_ExpectedItems);
        }

        private void Test_When_NoFilesMissing_Should_Return_ExpectedItems(
            IList<KeyValuePair<string, string>> input_HashItems,
            IList<FileInfo> input_Files,
            IList<KeyValuePair<FileInfo, string>> expected_ExpectedItems)
        {
            (IList<KeyValuePair<FileInfo, string>> result_ExpectedItems,
            IList<KeyValuePair<FileInfo, string>> result_MissingItems) = HashFileUtil.GetHashData(input_HashItems, input_Files);

            Assert.IsEmpty(result_MissingItems, "Missing items");

            Verify_Result_ContainsExpected(expected_ExpectedItems, result_ExpectedItems);
        }

        [TestCaseSource(nameof(GetCases_LessFilesThanExpected))]
        public void When_ExpectedFilesAreMissing__Should_Return_ItemsForExistingFiles(
            IList<KeyValuePair<string, string>> input_Items,
            IList<FileInfo> input_Files,
            IList<KeyValuePair<FileInfo, string>> expected_items,
            IList<KeyValuePair<FileInfo, string>> expected_Missingitems)
        {
            (IList<KeyValuePair<FileInfo, string>> result_ExpectedItems,
            IList<KeyValuePair<FileInfo, string>> _) = HashFileUtil.GetHashData(input_Items, input_Files);

            Verify_Result_ContainsExpected(expected_items, result_ExpectedItems);
        }

        [TestCaseSource(nameof(GetCases_LessFilesThanExpected))]
        public void When_ExpectedFilesAreMissing__Should_Return_MissingItems(
            IList<KeyValuePair<string, string>> input_Items,
            IList<FileInfo> input_Files,
            IList<KeyValuePair<FileInfo, string>> expected_items,
            IList<KeyValuePair<FileInfo, string>> expected_Missingitems)
        {
            (IList<KeyValuePair<FileInfo, string>> _,
            IList<KeyValuePair<FileInfo, string>> result_Missing) = HashFileUtil.GetHashData(input_Items, input_Files);

            Assert.IsNotEmpty(result_Missing);

            for (int i = 0; i < expected_Missingitems.Count; i++)
            {
                Assert.IsFalse(result_Missing.Count - 1 < i, "The missing-result should not contain more items than there are missing files");

                var target = expected_Missingitems[i];
                Assert.IsTrue(result_Missing.Any(x => x.Key.FullName == target.Key.FullName), $"Contains item {target.Key.Name}");

                var actualItem = result_Missing.First(x => x.Key.FullName == target.Key.FullName);

                Assert.AreEqual(target.Value, actualItem.Value, $"Hashes, index {i}");
            }
        }

        private static IEnumerable<TestCaseData> GetCases_LessFilesThanExpected()
        {
            var fileName1 = "Nirvana_1990-08-17_SBD1_t01.flac";
            var fileName2 = "Nirvana_1990-08-17_SBD1_t02.flac";
            var file1 = new FileInfo(fileName1);
            var file2 = new FileInfo(fileName2);

            var hash1 = "659e7ccaa5cb9d72bb508a382fd17fdf84e0eab412d9341fd10c14edd8722277";
            var hash2 = "5ea29a4b4ed755218a8fc5af7b026caaa4d98582ca21554de1e8f0c18d5a69a1";

            var missing1_fileName = "missing file";
            var missing1_hash = "1235ea29a4b4ed755218a8fc5af7b026caaa4d98582ca21554de1e8f0c18missing";

            yield return new TestCaseData(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(fileName1, hash1),
                    new KeyValuePair<string, string>(missing1_fileName, missing1_hash),
                    new KeyValuePair<string, string>(fileName2, hash2),
                },
                new FileInfo[] // input files
                {
                    file1,
                    file2
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2)
                },
                new List<KeyValuePair<FileInfo, string>> // expected missing items
                {
                    //new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(new FileInfo(missing1_fileName), missing1_hash)
                });

            yield return new TestCaseData(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(fileName1, hash1),
                    new KeyValuePair<string, string>(fileName2, hash2),
                },
                new FileInfo[0], // input files
                new KeyValuePair<FileInfo, string>[0], // expected items
                new List<KeyValuePair<FileInfo, string>> // expected missing items
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2)
                });
        }

        private void Verify_Result_ContainsExpected(
            IList<KeyValuePair<FileInfo, string>> expected_ExpectedItems,
            IList<KeyValuePair<FileInfo, string>> result_ExpectedItems)
        {
            for (int i = 0; i < expected_ExpectedItems.Count; i++)
            {
                var expected = expected_ExpectedItems[i];
                Assert.IsFalse(result_ExpectedItems.Count - 1 < i, "The result should not contain more items than there are in the source");

                var matchingFile = result_ExpectedItems.FirstOrDefault(x => x.Key == expected.Key);
                Assert.IsNotNull(matchingFile, $"The file should be present: {expected.Key.FullName}");
                Assert.AreEqual(expected.Value, matchingFile.Value, $"The hash should match");
            }
        }
    }
}