using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Hashing.Verification
{
    public class HashEntryMatching_NameBased_Tests
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

        static string FileToString(FileInfo file) => file.Name;

        [TestCaseSource(nameof(GetCases_SameNumberOfFiles_AsHashes))]
        public void When_All_FilesReferencedByHashlist_ArePresent__Must_Return_AllHashes_WithCorrespondingFiles_InOriginalHashOrder(string description, IList<KeyValuePair<string, string>> inputHashes, IList<FileInfo> inputFiles, IList<KeyValuePair<FileInfo, string>> expected)
        {
            var result = HashEntryMatching.MatchFilesToHashesNameBased(inputHashes, inputFiles).ToList();

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Key),
                expected.Select(x => x.Key),
                FileToString,
                "File infos");

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Value),
                expected.Select(x => x.Value),
                "Hash values");
        }

        [TestCaseSource(nameof(GetCases_AllMatches_WithExtraFiles))]
        public void When_All_FilesReferencedByHashlist_ArePresent_And_ThereAreExtraUnreferencedFiles__Must_IgnoreThem(string description, IList<KeyValuePair<string, string>> inputHashes, IList<FileInfo> inputFiles)
        {
            var expected = inputHashes.ToList();

            var result = HashEntryMatching.MatchFilesToHashesNameBased(inputHashes, inputFiles).ToList();

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Key.Name),
                expected.Select(x => x.Key),
                "File infos");

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Value),
                expected.Select(x => x.Value),
                "Hash values");
        }

        [TestCaseSource(nameof(GetCases_FewerFilesPresent_ThanExpected))]
        public void When_Some_ReferencedFilesAreMissing__Must_Return_Them_AsFileInfoWithSameName_InTheSamePositions(string description, IList<KeyValuePair<string, string>> inputHashes, IList<FileInfo> inputFiles, IList<KeyValuePair<FileInfo, string>> expected)
        {
            var result = HashEntryMatching.MatchFilesToHashesNameBased(inputHashes, inputFiles).ToList();

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Key.Name),
                expected.Select(x => x.Key.Name),
                "File infos");

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Value),
                expected.Select(x => x.Value),
                "Hash values");
        }

        [TestCaseSource(nameof(GetCases_SomeMissing_SomeExtra))]
        public void When_Some_ReferencedFilesAreMissing_And_ExtraFilesArePresent__Must_Return_Hashes_OnlyForReferencedFiles_ThatArePresent(string description, IList<KeyValuePair<string, string>> inputHashes, IList<FileInfo> inputFiles, IList<KeyValuePair<FileInfo, string>> expected)
        {
            var results = HashEntryMatching.MatchFilesToHashesNameBased(inputHashes, inputFiles).ToList();

            AssertThat.CollectionsMatchExactly(
                results.Select(x => x.Key.Name),
                expected.Select(x => x.Key.Name),
                "File infos");

            AssertThat.CollectionsMatchExactly(
                results.Select(x => x.Value),
                expected.Select(x => x.Value),
                "Hash values");
        }

        [TestCaseSource(nameof(GetCases_NoInputFilesPresent))]
        public void When_All_FilesReferencedByHashlist_AreMissing__Must_Return_Them_AsFileInfoWithSameName_InTheSamePositions(IList<KeyValuePair<string, string>> inputHashes)
        {
            var expectedNames = inputHashes.Select(x => x.Key).ToList();
            var expectedHashes = inputHashes.Select(x => x.Value).ToList();

            var result = HashEntryMatching.MatchFilesToHashesNameBased(inputHashes, Array.Empty<FileInfo>()).ToList();

            Assert.IsNotEmpty(result, "Missing items");

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Key.Name),
                expectedNames,
                "File infos");

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Value),
                expectedHashes,
                "Hash values");
        }

        [TestCaseSource(nameof(GetCases_AllMissing_SomeExtra))]
        public void When_All_FilesReferencedByHashlist_AreMissing_AndExtraFilesArePresent__Must_Return_OnlyReferencedFiles_AsFileInfoWithSameName(IList<KeyValuePair<string, string>> inputHashes, IList<FileInfo> inputFiles)
        {
            var expectedNames = inputHashes.Select(x => x.Key).ToList();
            var expectedHashes = inputHashes.Select(x => x.Value).ToList();

            var result = HashEntryMatching.MatchFilesToHashesNameBased(inputHashes, inputFiles).ToList();

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Key.Name),
                expectedNames,
                "File infos");

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Value),
                expectedHashes,
                "Hash values");
        }

        [Test]
        public void When_ReturningMissingItems_Must_ReturnThem_AsFileInfo_And_NameMustEqualOriginalName()
        {
            var inputHashes = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(filename1, "hash1"),
                new KeyValuePair<string, string>(filename2, "hash2"),
                new KeyValuePair<string, string>(filename3, "hash3")
            };

            var result = HashEntryMatching.MatchFilesToHashesNameBased(inputHashes, new FileInfo[0]).ToList();

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Key.Name),
                new string[]
                { filename1, filename2, filename3 },
                "File infos");
        }

        private static IEnumerable<TestCaseData> GetCases_FewerFilesPresent_ThanExpected()
        {
            var file1 = new FileInfo($@"{path}\{filename1}");
            var file2 = new FileInfo($@"{path}\{filename2}");
            var file3 = new FileInfo($@"{path}\{filename3}");
            var file4 = new FileInfo($@"{path}\{filename4}");
            var file5 = new FileInfo($@"{path}\{filename5}");
            var file6 = new FileInfo($@"{path}\{filename6}");

            yield return new TestCaseData(
                "Last file missing",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename3, hash3) //missing
                },
                new FileInfo[] // input files
                {
                    file1,
                    file2
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename3), hash3),
                });

            yield return new TestCaseData(
                "First file missing",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename3, hash3), //missing
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename2, hash2)
                },
                new FileInfo[] // input files
                {
                    file1,
                    file2
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename3), hash3),
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2)
                });

            yield return new TestCaseData(
                "Buncha files missing",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename3, hash3), //missing
                    new KeyValuePair<string, string>(filename4, hash4), //missing
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename5, hash5),
                    new KeyValuePair<string, string>(filename6, hash6), //missing
                },
                new FileInfo[] // input files
                {
                    file1, file2, file5
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename3), hash3),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename4), hash4),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(file5, hash5),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename6), hash6)
                });

            yield return new TestCaseData(
                "Buncha files missing, Files out of order",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename3, hash3), //missing
                    new KeyValuePair<string, string>(filename4, hash4), //missing
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename5, hash5),
                    new KeyValuePair<string, string>(filename6, hash6), //missing
                },
                new FileInfo[] // input files
                {
                    file2, file5, file1
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename3), hash3),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename4), hash4),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(file5, hash5),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename6), hash6)
                });
        }

        private static IEnumerable<TestCaseData> GetCases_SomeMissing_SomeExtra()
        {
            var file1 = new FileInfo($@"{path}\{filename1}");
            var file2 = new FileInfo($@"{path}\{filename2}");
            var file3 = new FileInfo($@"{path}\{filename3}");
            var file4 = new FileInfo($@"{path}\{filename4}");
            var file5 = new FileInfo($@"{path}\{filename5}");
            var file6 = new FileInfo($@"{path}\{filename6}");

            yield return new TestCaseData(
                "One file missing, One file extra",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename3, hash3) //missing
                },
                new FileInfo[] // input files
                {
                    file1,
                    file2,
                    file5 //extra
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename3), hash3)
                });

            yield return new TestCaseData(
                "Two missing, One extra",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename2, hash2), //missing
                    new KeyValuePair<string, string>(filename3, hash3),
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename4, hash4), //missing
                },
                new FileInfo[] // input files
                {
                    file1,
                    file3,
                    file5 //extra
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename2), hash2),
                    new KeyValuePair<FileInfo, string>(file3, hash3),
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename4), hash4)
                });

            yield return new TestCaseData(
                "One missing, Two extra",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename5, hash5),
                    new KeyValuePair<string, string>(filename4, hash4), //missing
                    new KeyValuePair<string, string>(filename1, hash1)
                },
                new FileInfo[] // input files
                {
                    file1,
                    file2,
                    file3, //extra
                    file5,
                    file6 //extra
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(file5, hash5),
                    new KeyValuePair<FileInfo, string>(new FileInfo(filename4), hash4),
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                });
        }

        private static IEnumerable<TestCaseData> GetCases_AllMissing_SomeExtra()
        {
            var file1 = new FileInfo($@"{path}\{filename1}");
            var file2 = new FileInfo($@"{path}\{filename2}");
            var file3 = new FileInfo($@"{path}\{filename3}");
            var file4 = new FileInfo($@"{path}\{filename4}");
            var file5 = new FileInfo($@"{path}\{filename5}");

            yield return new TestCaseData(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename4, hash4)
                },
                new FileInfo[] // input files
                {
                    file3,
                    file5,
                    file1
                });

            yield return new TestCaseData(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename3, hash3)
                },
                new FileInfo[] // input files
                {
                    file4,
                    file5
                });
        }

        private static IEnumerable<TestCaseData> GetCases_NoInputFilesPresent()
        {
            yield return new TestCaseData(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename2, hash2),
                });
        }

        private static IEnumerable<TestCaseData> GetCases_SameNumberOfFiles_AsHashes()
        {
            var file1 = new FileInfo($@"{path}\{filename1}");
            var file2 = new FileInfo($@"{path}\{filename2}");
            var file3 = new FileInfo($@"{path}\{filename3}");
            var file4 = new FileInfo($@"{path}\{filename4}");

            yield return new TestCaseData(
                "Single file",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1)
                },
                new FileInfo[] // input files
                {
                    file1
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1)
                });

            yield return new TestCaseData(
                "Multiple files",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename3, hash3),
                    new KeyValuePair<string, string>(filename4, hash4),
                },
                new FileInfo[] // input files
                {
                    file1, file2, file3, file4
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(file3, hash3),
                    new KeyValuePair<FileInfo, string>(file4, hash4)
                });

            yield return new TestCaseData(
                "Multiple files; Files out of order",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename3, hash3),
                    new KeyValuePair<string, string>(filename4, hash4),
                },
                new FileInfo[] // input files
                {
                    file4, file1, file3, file2
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file1, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(file3, hash3),
                    new KeyValuePair<FileInfo, string>(file4, hash4)
                });

            yield return new TestCaseData(
                "Multiple files, Hashes in non-filename-based order",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename4, hash1),
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename1, hash3),
                    new KeyValuePair<string, string>(filename3, hash4),
                },
                new FileInfo[] // input files
                {
                    file1, file2, file3, file4
                },
                new List<KeyValuePair<FileInfo, string>> // expected items
                {
                    new KeyValuePair<FileInfo, string>(file4, hash1),
                    new KeyValuePair<FileInfo, string>(file2, hash2),
                    new KeyValuePair<FileInfo, string>(file1, hash3),
                    new KeyValuePair<FileInfo, string>(file3, hash4)
                });
        }

        private static IEnumerable<TestCaseData> GetCases_AllMatches_WithExtraFiles()
        {
            var file1 = new FileInfo($@"{path}\{filename1}");
            var file2 = new FileInfo($@"{path}\{filename2}");
            var file3 = new FileInfo($@"{path}\{filename3}");
            var file4 = new FileInfo($@"{path}\{filename4}");
            var file5 = new FileInfo($@"{path}\{filename5}");

            yield return new TestCaseData(
                "Single hash",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1)
                },
                new FileInfo[] // input files
                {
                    file1,
                    file2 //extra
                });

            yield return new TestCaseData(
                "Multiple hashes",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename3, hash3),
                },
                new FileInfo[] // input files
                {
                    file1,
                    file2,
                    file3,
                    file4, //extra
                    file5 //extra
                });

            yield return new TestCaseData(
                "Multiple hashes; Files out of order",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename1, hash1),
                    new KeyValuePair<string, string>(filename2, hash2),
                    new KeyValuePair<string, string>(filename3, hash3),
                },
                new FileInfo[] // input files
                {
                    file4, //extra
                    file1,
                    file3,
                    file2
                });

            yield return new TestCaseData(
                "Multiple files, Hashes in non-filename-based order",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(filename4, hash1),
                    new KeyValuePair<string, string>(filename1, hash3),
                    new KeyValuePair<string, string>(filename3, hash4),
                },
                new FileInfo[] // input files
                {
                    file1,
                    file2, //extra 
                    file3,
                    file4,
                    file5 //extra
                });
        }
    }
}