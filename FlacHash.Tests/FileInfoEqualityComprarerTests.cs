using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Andy.FlacHash
{
    public class FileInfoEqualityComprarerTests
    {
        private FileInfoEqualityComprarer target = new FileInfoEqualityComprarer();

        [TestCaseSource(nameof(GetMatches))]
        public void Equals__True_When_FullPaths_Match_Ignoring_LetterCasing(string file1, string file2)
        {
            var result = target.Equals(new FileInfo(file1), new FileInfo(file2));
            Assert.True(result);
        }

        [TestCaseSource(nameof(GetNoMatches))]
        public void Equals__False_When_FullPaths_DontMatch_Ignoring_LetterCasing(string file1, string file2)
        {
            var result = target.Equals(new FileInfo(file1), new FileInfo(file2));
            Assert.False(result);
        }

        [TestCaseSource(nameof(GetMatches))]
        public void Equals__True_When_BothFiles_Are_TheSameInstance(string file1, string file2)
        {
            var file = new FileInfo(file1);
            var result = target.Equals(file, file);
            Assert.True(result);
        }

        [TestCaseSource(nameof(GetMatches))]
        public void GetHashCode__Identical_Hashcodes_ForFiles_Where_FullPaths_Match_Ignoring_LetterCasing(string file1, string file2)
        {
            var one = target.GetHashCode(new FileInfo(file1));
            var two = target.GetHashCode(new FileInfo(file2));
            Assert.AreEqual(one, two);
        }

        [TestCaseSource(nameof(GetNoMatches))]
        public void Equals__Different_Hashcodes_ForFiles_Where_FullPaths_DontMatch_Ignoring_LetterCasing(string file1, string file2)
        {
            var one = target.GetHashCode(new FileInfo(file1));
            var two = target.GetHashCode(new FileInfo(file2));
            Assert.AreNotEqual(one, two);
        }

        [TestCaseSource(nameof(GetMatches))]
        public void GetHashCode__Identical_Hashcodes_ForFiles_That_Are_TheSameInstance(string file1, string file2)
        {
            var file = new FileInfo(file1);
            var one = target.GetHashCode(file);
            var two = target.GetHashCode(file);
            Assert.AreEqual(one, two);
        }

        private static IEnumerable<TestCaseData> GetMatches()
        {
            yield return new TestCaseData("asd.flac", "Asd.Flac");
            yield return new TestCaseData(@"c:\asd.flac", @"c:\Asd.Flac");
            yield return new TestCaseData(@"c:\flac\asd.flac", @"C:\FLac\asd.flac");
            yield return new TestCaseData(@"c:\flac\asd.flac", @"C:\FLac\asd.flac");
            yield return new TestCaseData(@"c:\flac\asd.flac", @"c:/FLac/asd.flac");

            var file = new FileInfo("sad.flac");
            yield return new TestCaseData(file.Name, file.FullName);
        }
        
        private static IEnumerable<TestCaseData> GetNoMatches()
        {
            yield return new TestCaseData("asd.flac", "asd.lac");
            yield return new TestCaseData(@"c:\asd.flac", @"d:\asd.flac");
        }
    }
}