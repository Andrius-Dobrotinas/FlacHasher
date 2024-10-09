using Andy.FlacHash.Hashing;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Andy.FlacHash.Cmd.Verification;

namespace Andy.FlacHash.Cmd
{
    public class Verification_Tests
    {
        Mock<IFileSearch> filesearch;

        [SetUp]
        public void Setup()
        {
            filesearch = new Mock<IFileSearch>();
        }

        [Test]
        public void When_Neither_Hashfile_Nor_Directory_AreSpecified__Must_Return_Null()
        {
            var @params = new Params
            {
                HashFile = null,
                InputDirectory = null
            };
            var result = Verification.GetHashFile(@params, filesearch.Object);

            Assert.IsNull(result);
        }

        [TestCase("c:\\file.hash", null)]
        [TestCase("c:\\file.hash", "c:\\flac")]
        [TestCase("c:\\fyle", null)]
        [TestCase("c:\\fyle", "c:\\muzak")]
        public void Hashfile_SpecifiedAs_AbsolutePath__RegardlessOfDirectory__Must_Return_This_File(string filename, string dirname)
        {
            var @params = new Params
            {
                HashFile = filename,
                InputDirectory = dirname
            };
            var result = Verification.GetHashFile(@params, filesearch.Object);

            Assert.NotNull(result);
            Assert.AreEqual(filename, result.FullName);
        }

        [TestCase("file.hash")]
        [TestCase("file")]
        public void Hashfile_SpecifiedAs_JustFileName__NoDirectorySpecified__Must_Return_This_File_InThe_CurrentDirectory(string filename)
        {
            var @params = new Params
            {
                HashFile = filename
            };
            var result = Verification.GetHashFile(@params, filesearch.Object);

            Assert.NotNull(result);

            Assert.AreEqual(filename, result.Name);
            Assert.AreEqual(new DirectoryInfo(Directory.GetCurrentDirectory()).FullName, result.Directory.FullName);
        }

        [TestCase("file.hash", "d:\\dir")]
        [TestCase("hasheesh.md5", "d:\\muzak\\flac\\directory")]
        [TestCase("hasheesh", "d:\\muzak\\flac\\directory")]
        public void Hashfile_SpecifiedAs_JustFileName__DirectorySpecified__Must_Return_This_File_InThe_Specified_Directory(string filename, string dirname)
        {
            var @params = new Params
            {
                HashFile = filename,
                InputDirectory = dirname
            };
            var result = Verification.GetHashFile(@params, filesearch.Object);

            Assert.NotNull(result);

            Assert.AreEqual(filename, result.Name);
            Assert.AreEqual(new DirectoryInfo(dirname).FullName, result.Directory.FullName);
        }

        [TestCase("d:\\dir")]
        [TestCase("d:\\dir\\")]
        [TestCase("d:\\muzak\\flac\\directory")]
        public void No_Hashfile_Specified__Must_Scan_The_SpecifiedDirectory_For_Files(string dirname)
        {
            var @params = new Params
            {
                InputDirectory = dirname
            };
            var result = Verification.GetHashFile(@params, filesearch.Object);

            filesearch.Verify(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<string>()),
                Times.Once,
                "Must search the directory");

            filesearch.Verify(
                x => x.FindFiles(
                    It.Is<DirectoryInfo>(
                        arg => arg.FullName == dirname),
                    It.IsAny<string>()),
                "Must search the Specified Directory");

            filesearch.Verify(
                x => x.FindFiles(
                    It.IsAny<DirectoryInfo>(),
                    It.Is<string>(
                        arg => arg == "*")),
                "Must search the directory for All file types");
        }

        [TestCaseSource(nameof(GetCases_HashfileLookup))]
        public void No_Hashfile_Specified__Must_Pick_Any_Hashfile_Of_Accepted_Filetypes__From_The_SpecifiedDirectory(string description, string[] nameOfFilesInTheDirectory, string[] acceptedHashTypes, string[] acceptableHashfileNames)
        {
            var dirname = "c:\\muzak\\narvana\\boot1";

            var filesInDir = nameOfFilesInTheDirectory.Select(filename => new FileInfo(Path.Combine(dirname, filename))).ToArray();
            var acceptableFiles = acceptableHashfileNames.Select(filename => filesInDir.First(x => x.Name == filename)).ToArray();

            filesearch.Setup(
                    x => x.FindFiles(It.IsAny<DirectoryInfo>(), It.IsAny<string>()))
                .Returns(filesInDir);

            var @params = new Params
            {
                InputDirectory = dirname,
                HashfileExtensions = acceptedHashTypes
            };

            var result = Verification.GetHashFile(@params, filesearch.Object);

            AssertThat.IsIn(result, acceptableFiles);
        }

        static IEnumerable<TestCaseData> GetCases_HashfileLookup()
        {
            {
                var files = new[]
                {
                    "01 - Smells Like Teen Spirit.flac",
                    "02 - In Bloom.flac",
                    "fingerprint.md5"
                };

                yield return new TestCaseData(
                    "One hash file in the dir, one expected filetype",
                    files,
                    new[] { "md5" },
                    new[]
                    {
                        "fingerprint.md5"
                    });

                yield return new TestCaseData(
                    "One hash file in the dir, multiple expected filetypes",
                    files,
                    new[] { "hash", "md5", "ext" },
                    new[]
                    {
                        "fingerprint.md5"
                    });
            }

            yield return new TestCaseData(
                "Multiple hash files in the dir, all of the same filetype",
                new[]
                {
                    "01 - Smells Like Teen Spirit.flac",
                    "02 - In Bloom.flac",
                    "fingerprint-psych.md5",
                    "fingerprint-copy.md5",
                    "fingerprint.md5"
                },
                new[] { "hash", "md5" },
                new[]
                {
                    "fingerprint-psych.md5",
                    "fingerprint-copy.md5",
                    "fingerprint.md5"
                });

            {
                var files = new[]
                {
                    "01 - Blew.flac",
                    "03 - Floyd The Barber.flac",
                    "files.md5",
                    "files.hash",
                    "fingerprint-copy.md5",
                    "fingerprint-copy-0.hash",
                    "readme.txt",
                    "fingerprint.md5"
                };

                yield return new TestCaseData(
                    "Multiple hash files in the dir, multiple filetypes, expected two types: HASH & MD5",
                    files,
                    new[] { "md5", "hash" },
                    new[]
                    {
                        "files.md5",
                        "files.hash",
                        "fingerprint-copy.md5",
                        "fingerprint-copy-0.hash",
                        "fingerprint.md5"
                    });

                yield return new TestCaseData(
                    "Multiple hash files in the dir, multiple filetypes, expected just one type: MD5",
                    files,
                    new[] { "md5" },
                    new[]
                    {
                        "files.md5",
                        "fingerprint-copy.md5",
                        "fingerprint.md5"
                    });

                yield return new TestCaseData(
                    "Multiple hash files in the dir, multiple filetypes, expected just one type: HASH",
                    files,
                    new[] { "hash" },
                    new[]
                    {
                        "files.hash",
                        "fingerprint-copy-0.hash",
                    });
            }
        }

        class Params : IHashfileParams
        {
            public string HashFile { get; set; }
            public string[] HashfileExtensions { get; set; }
            public string HashfileEntrySeparator { get; set; }
            public string InputDirectory { get; set; }
        }
    }
}