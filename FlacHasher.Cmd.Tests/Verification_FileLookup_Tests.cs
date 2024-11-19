using Andy.IO;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Application.Cmd
{
    public class Verification_FileLookup_Tests
    {
        Mock<IFileSearch> filesearch;

        [SetUp]
        public void Setup()
        {
            filesearch = new Mock<IFileSearch>();
        }

        [TestCase("hashfile.one", "x")]
        [TestCase("c:\\file\\my.hash.file", "flask")]
        [TestCase("hashfile.one", "ext")]
        public void Hashfile_Specified_NoInputDirOrFiles_And_HashfileIs_PositionBased__Must__Search_For_InputFiles_In_Resolved_Hashfile_Directory(string hashfilePath, string targetFileExtension)
        {
            var @params = new Params
            {
                HashFile = "c:\\hash.file",
                InputDirectory = null,
                InputFiles = null,
                TargetFileExtension = targetFileExtension
            };

            var filehashmap_file = new FileHashMap(Array.Empty<KeyValuePair<string, string>>(), hasNoFileNames: true);
            var hashfile = new FileInfo(hashfilePath);
            var expectedDirectoryPath = hashfile.Directory.FullName;
            var expectedLookupExtensions = new string[] { targetFileExtension };
            var result = Verification.FindFiles(hashfile, filehashmap_file, @params, filesearch.Object);

            filesearch.Verify(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<string[]>()),
                Times.Once,
                "Must search for files");

            filesearch.Verify(
                x => x.FindFiles(
                    It.Is<DirectoryInfo>(arg => expectedDirectoryPath.Equals(arg.FullName, StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<string[]>()),
                "Must search files in the hashfile's directory");

            filesearch.Verify(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.Is<string[]>(arg => arg != null && arg.SequenceEqual(expectedLookupExtensions))),
                "Must search for files of pre-configured type");
        }

        [TestCase("c:\\file\\01.flac")]
        [TestCase("c:\\file\\04.flac", "c:\\file\\06.flac")]
        public void Hashfile_Specified_NoInputDirOrFiles_And_HashfileIs_PositionBased__Must__Return_LookedUpFiles(params string[] files)
        {
            var @params = new Params
            {
                HashFile = "c:\\files\\hash.file",
                InputDirectory = null,
                InputFiles = null
            };

            var filehashmap_file = new FileHashMap(Array.Empty<KeyValuePair<string, string>>(), hasNoFileNames: true);
            var hashfile = new FileInfo("c:\\files\\hash.file");

            var expectedFiles = files.Select(x => new FileInfo(x));

            filesearch.Setup(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<string[]>()))
                .Returns(expectedFiles);

            var result = Verification.FindFiles(hashfile, filehashmap_file, @params, filesearch.Object);

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.FullName).OrderBy(x => x),
                expectedFiles.Select(x => x.FullName).OrderBy(x => x));
        }

        [TestCaseSource(nameof(GetCases_FileLookup1))]
        public void Hashfile_Specified_NoInputDirOrFiles_And_HashfileIs_FilenameBased__Must__Return_AllFiles_DefinedInHashfile_With_BasePath_SameAsHashfile(string hashfilePath, string expectedBasePath, IDictionary<string, string> hashfileEntries)
        {
            var @params = new Params
            {
                HashFile = hashfilePath,
                InputDirectory = null,
                InputFiles = null
            };

            var expectedFiles = hashfileEntries.Keys;

            var filehashmap_file = new FileHashMap(hashfileEntries.ToArray(), hasNoFileNames: false);
            var hashfile = new FileInfo(hashfilePath);
            var result = Verification.FindFiles(hashfile, filehashmap_file, @params, filesearch.Object);

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.Name).OrderBy(x => x), 
                expectedFiles.OrderBy(x => x),
                "file names");

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.DirectoryName).OrderBy(x => x),
                expectedFiles.Select(x => expectedBasePath),
                "file directory");
        }

        [TestCase("c:\\hasheesh\\hash.hash", "c:\\d\\muzak", "1.flac", "2.flac")]
        [TestCase("c:\\d\\a.txt", "e:\\mp3\\flac", "four.flac", "2.flac", "five.flac")]
        public void Hashfile_And_InputDir_Specified__Must__Search_For_InputFiles_In_TheSpecified_InputDirectory(string hashfilePath, string inputDirPath, params string[] filepaths)
        {
            var @params = new Params
            {
                HashFile = hashfilePath,
                InputDirectory = inputDirPath,
                InputFiles = null
            };

            var expectedFiles = filepaths.Select(x => new FileInfo(x));

            filesearch.Setup(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<string[]>()))
                .Returns(expectedFiles);

            var filehashmap_file = new FileHashMap(Array.Empty<KeyValuePair<string, string>>(), hasNoFileNames: false);
            var hashfile = new FileInfo(hashfilePath);
            var result = Verification.FindFiles(hashfile, filehashmap_file, @params, filesearch.Object);

            filesearch.Verify(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<string[]>()),
                Times.Once,
                "Must search for files");

            filesearch.Verify(
                x => x.FindFiles(
                    It.Is<DirectoryInfo>(arg => inputDirPath.Equals(arg.FullName, StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<string[]>()),
                "Must search files in the specified directory");

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.FullName).OrderBy(x => x),
                expectedFiles.Select(x => x.FullName).OrderBy(x => x),
                "Return files returned by the search thing");
        }

        [TestCase("c:\\hasheesh\\hash.hash", "1.flac", "2.flac")]
        [TestCase("c:\\d\\a.txt", "four.flac", "2.flac", "five.flac")]
        public void Hashfile_And_InputFiles_Specified__Must__Use_TheSuppliedInputFiles(string hashfilePath, params string[] filepaths)
        {
            var @params = new Params
            {
                HashFile = hashfilePath,
                InputDirectory = null,
                InputFiles = filepaths
            };

            var expectedFiles = filepaths.Select(x => new FileInfo(x));

            filesearch.Setup(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<string[]>()))
                .Returns(expectedFiles);

            var filehashmap_file = new FileHashMap(Array.Empty<KeyValuePair<string, string>>(), hasNoFileNames: false);
            var hashfile = new FileInfo(hashfilePath);
            var result = Verification.FindFiles(hashfile, filehashmap_file, @params, filesearch.Object);

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.FullName).OrderBy(x => x),
                expectedFiles.Select(x => x.FullName).OrderBy(x => x),
                "Return files returned by the search thing");
        }

        [TestCase("c:\\directory", "flac", "1.flac", "2.flac")]
        [TestCase("d:\\e\\m", "x", "four.flac", "2.flac", "five.flac")]
        public void NoHashfile_And_OnlyInputDir_Specified__Must__Search_ForFiles_InTheDir(string dir, string targetFileExtension, params string[] filepaths)
        {
            var expectedDirectoryPath = new DirectoryInfo(dir).FullName;
            var expectedFiles = filepaths.Select(x => new FileInfo(x));
            var expectedLookupExtensions = new string[] { targetFileExtension };
            
            var @params = new Params
            {
                HashFile = null,
                InputDirectory = dir,
                InputFiles = filepaths,
                TargetFileExtension = targetFileExtension
            };

            filesearch.Setup(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<string[]>()))
                .Returns(expectedFiles);

            var filehashmap_file = new FileHashMap(Array.Empty<KeyValuePair<string, string>>(), hasNoFileNames: false);
            var result = Verification.FindFiles(resolvedHashfile: null, filehashmap_file, @params, filesearch.Object);

            filesearch.Verify(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<string[]>()),
                Times.Once,
                "Must search for files");

            filesearch.Verify(
                x => x.FindFiles(
                    It.Is<DirectoryInfo>(arg => expectedDirectoryPath.Equals(arg.FullName, StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<string[]>()),
                "Must search files in the hashfile's directory");

            filesearch.Verify(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.Is<string[]>(arg => arg != null && arg.SequenceEqual(expectedLookupExtensions))),
                "Must search for files of pre-configured type");

            AssertThat.CollectionsMatchExactly(
                result.Select(x => x.FullName).OrderBy(x => x),
                expectedFiles.Select(x => x.FullName).OrderBy(x => x),
                "Return files returned by the search thing");
        }

        static IEnumerable<TestCaseData> GetCases_FileLookup1()
        {
            {
                yield return new TestCaseData(
                    "hashfile.one",
                    Directory.GetCurrentDirectory(),
                    new Dictionary<string, string>
                    {
                        { "01.flac", "hash1" },
                        { "02.flac", "hash2" }
                    });

                yield return new TestCaseData(
                    "d:\\muzak\\one\\hashfile.one",
                    "d:\\muzak\\one",
                    new Dictionary<string, string>
                    {
                        { "01.flac", "hash1" },
                        { "02.flac", "hash2" }
                    });

                yield return new TestCaseData(
                    "c:\\muzak\\elsewhere\\file.two.txt",
                    "c:\\muzak\\elsewhere",
                    new Dictionary<string, string>
                    {
                        { "uno.flac", "hash11" },
                        { "dos.flac", "hash22" },
                        { "tres.flac", "hash33" }
                    });
            }
        }

        class Params : VerificationParameters
        {
            public Params()
            {
                HashfileExtensions = Array.Empty<string>(); 
            }
        }
    }
}