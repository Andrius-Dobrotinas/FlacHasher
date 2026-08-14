using Andy.Cmd.Parameter;
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

        [TestCaseSource(nameof(GetCases_SearchInResolvedHashfileDirectory))]
        public void Specified_Hashfile_NoInputDir_NoInputFiles_And_HashfileIs_PositionBased__Must__Search_For_InputFiles_In_Resolved_Hashfile_Directory(string hashfilePath, string targetFileExtension)
        {
            var @params = new Params
            {
                HashFile = OperatingSystem.IsWindows()
                    ? "c:\\hash.file"
                    : "/hash.file",
                InputDirectory = null,
                InputFiles = null,
                TargetFileExtensions = new string[] { targetFileExtension }
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

        [Test]
        public void When_Hashfile_Is_PositionBased__Must_Require_TargetFileExtension()
        {
            var @params = new Params
            {
                HashFile = OperatingSystem.IsWindows()
                    ? "c:\\files\\hash.file"
                    : "/files/hash.file",
                InputDirectory = null,
                InputFiles = null,
                TargetFileExtensions = null
            };

            var filehashmap_file = new FileHashMap(Array.Empty<KeyValuePair<string, string>>(), hasNoFileNames: true);
            var hashfile = new FileInfo("c:\\files\\hash.file");

            filesearch.Setup(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<string[]>()))
                .Returns([]);

            var exception = Assert.Throws<ParameterMissingException>(() => Verification.FindFiles(hashfile, filehashmap_file, @params, filesearch.Object));

            Assert.AreEqual(typeof(VerificationParameters).GetProperty(nameof(VerificationParameters.TargetFileExtensions)), exception.ParameterProperty);
        }

        [TestCaseSource(nameof(GetCases_PositionBased_ReturnLookedUpFiles))]
        public void Specified_Hashfile_NoInputDir_NoInputFiles_And_HashfileIs_PositionBased__Must__Return_LookedUpFiles(params string[] files)
        {
            var @params = new Params
            {
                HashFile = OperatingSystem.IsWindows() 
                    ? "c:\\files\\hash.file"
                    : "/files/hash.file",
                InputDirectory = null,
                InputFiles = null,
                TargetFileExtensions = ["flac"]
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
        public void Specified_Hashfile_NoInputDir_NoInputFiles_And_HashfileIs_FilenameBased__Must__Return_AllFiles_DefinedInHashfile_With_BasePath_SameAsHashfile(string hashfilePath, string expectedBasePath, IDictionary<string, string> hashfileEntries)
        {
            var @params = new Params
            {
                HashFile = hashfilePath,
                InputDirectory = null,
                InputFiles = null,
                TargetFileExtensions = ["flac"]
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

        [TestCaseSource(nameof(GetCases_HashfileAndInputDir))]
        public void Specified_Hashfile_And_InputDir__Must__Search_For_InputFiles_In_TheSpecified_InputDirectory(string hashfilePath, string inputDirPath, params string[] filepaths)
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

        [TestCaseSource(nameof(GetCases_HashfileAndInputFiles))]
        public void Specified_Hashfile_And_InputFiles__Must__Use_TheSuppliedInputFiles(string hashfilePath, params string[] filepaths)
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

        [TestCaseSource(nameof(GetCases_NoHashfileOnlyInputDir))]
        public void Specified_NoHashfile_OnlyInputDir__Must__Search_ForFiles_InTheDir(string dir, string targetFileExtension, params string[] filepaths)
        {
            var expectedDirectoryPath = new DirectoryInfo(dir).FullName;
            var expectedFiles = filepaths.Select(x => new FileInfo(x));
            var expectedLookupExtensions = new string[] { targetFileExtension };
            
            var @params = new Params
            {
                HashFile = null,
                InputDirectory = dir,
                InputFiles = filepaths,
                TargetFileExtensions = new string[] { targetFileExtension }
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

        static IEnumerable<TestCaseData> GetCases_SearchInResolvedHashfileDirectory()
        {
            yield return new TestCaseData("hashfile.one", "x");
            yield return new TestCaseData("hashfile.one", "ext");
            yield return new TestCaseData(
                OperatingSystem.IsWindows() ? "c:\\file\\my.hash.file" : "/file/my.hash.file",
                "flask");
        }

        static IEnumerable<TestCaseData> GetCases_PositionBased_ReturnLookedUpFiles()
        {
            yield return new TestCaseData(
                (object)new[] { OperatingSystem.IsWindows() ? "c:\\file\\01.flac" : "/file/01.flac" });
            yield return new TestCaseData(
                (object)new[]
                {
                    OperatingSystem.IsWindows() ? "c:\\file\\04.flac" : "/file/04.flac",
                    OperatingSystem.IsWindows() ? "c:\\file\\06.flac" : "/file/06.flac"
                });
        }

        static IEnumerable<TestCaseData> GetCases_HashfileAndInputDir()
        {
            yield return new TestCaseData(
                OperatingSystem.IsWindows() ? "c:\\hasheesh\\hash.hash" : "/hasheesh/hash.hash",
                OperatingSystem.IsWindows() ? "c:\\d\\muzak" : "/d/muzak",
                new[] { "1.flac", "2.flac" });
            yield return new TestCaseData(
                OperatingSystem.IsWindows() ? "c:\\d\\a.txt" : "/d/a.txt",
                OperatingSystem.IsWindows() ? "e:\\mp3\\flac" : "/mp3/flac",
                new[] { "four.flac", "2.flac", "five.flac" });
        }

        static IEnumerable<TestCaseData> GetCases_HashfileAndInputFiles()
        {
            yield return new TestCaseData(
                OperatingSystem.IsWindows() ? "c:\\hasheesh\\hash.hash" : "/hasheesh/hash.hash",
                new[] { "1.flac", "2.flac" });
            yield return new TestCaseData(
                OperatingSystem.IsWindows() ? "c:\\d\\a.txt" : "/d/a.txt",
                new[] { "four.flac", "2.flac", "five.flac" });
        }

        static IEnumerable<TestCaseData> GetCases_NoHashfileOnlyInputDir()
        {
            yield return new TestCaseData(
                OperatingSystem.IsWindows() ? "c:\\directory" : "/directory",
                "flac",
                new[] { "1.flac", "2.flac" });
            yield return new TestCaseData(
                OperatingSystem.IsWindows() ? "d:\\e\\m" : "/e/m",
                "x",
                new[] { "four.flac", "2.flac", "five.flac" });
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
                    OperatingSystem.IsWindows() ? "d:\\muzak\\one\\hashfile.one" : "/muzak/one/hashfile.one",
                    OperatingSystem.IsWindows() ? "d:\\muzak\\one" :  "/muzak/one",
                    new Dictionary<string, string>
                    {
                        { "01.flac", "hash1" },
                        { "02.flac", "hash2" }
                    });

                yield return new TestCaseData(
                    OperatingSystem.IsWindows() ? "c:\\muzak\\elsewhere\\file.two.txt" : "/muzak/elsewhere/file.two.txt",
                    OperatingSystem.IsWindows() ? "c:\\muzak\\elsewhere" : "/muzak/elsewhere",
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