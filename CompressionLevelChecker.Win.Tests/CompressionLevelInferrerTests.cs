using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public class CompressionLevelInferrerTests
    {
        CompressionLevelInferrer target;
        Mock<ICompressedSizeService> encoder;
        Mock<IFileInfoSizeGetter> fileSizeGetter;

        [SetUp]
        public void Setup() {
            encoder = new Mock<ICompressedSizeService>();
            fileSizeGetter = new Mock<IFileInfoSizeGetter>();
        }

        private void CreateTarget(uint minCompressionLevel, uint maxCompressionLevel)
        {
            target = new CompressionLevelInferrer(
                encoder.Object,
                fileSizeGetter.Object,
                minCompressionLevel, 
                maxCompressionLevel);
        }

        [TestCase(1, 2)]
        [TestCase(10, 11)]
        public void When_TargetCompressionLevelIs_HigherThanMaximumLevelForTheEncoder__Must_ThrowAnException(
            int maxCompressionLevel,
            int targetCompressionLevel)
        {
            CreateTarget(uint.MinValue, (uint)maxCompressionLevel);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => target.InferCompressionLevel(new FileInfo("z"), (uint)targetCompressionLevel));
        }

        [TestCase(1, 0)]
        [TestCase(2, 1)]
        [TestCase(10, 9)]
        public void When_TargetCompressionLevelIs_LowerThanMinimumLevelForTheEncoder__Must_ThrowAnException(
            int minCompressionLevel,
            int targetCompressionLevel)
        {
            CreateTarget((uint)minCompressionLevel, uint.MaxValue);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => target.InferCompressionLevel(new FileInfo("y"), (uint)targetCompressionLevel));
        }

        [TestCaseSource(nameof(GetHappyCases))]
        public void When_TargetCompressionLevelIs_Correct__Should_Return_TheLevel_InASingleValueRange(
            uint targetCompressionLevel,
            long fileSize)
        {
            CreateTarget(uint.MinValue, uint.MaxValue);

            var expectedResult = new Range<uint>(targetCompressionLevel);

            Setup_OriginalFileSize(fileSize);
            Setup_EncodedSize(fileSize);

            var result = target.InferCompressionLevel(new FileInfo("s"), targetCompressionLevel);

            Assert.AreEqual(expectedResult.MaxValue, result.MaxValue, "Max value");
            Assert.AreEqual(expectedResult.MinValue, result.MinValue, "Min value");
        } 

        [TestCaseSource(nameof(Get_WhereFileIsBiggerThanTheTargetCompressionLevel))]
        public void When_TargetCompressionLevelIs_GreaterThanOriginal__Should_KeepGettingRecodedFileSize_Until_CompressionLevelIsFound(
            uint minCompressionLevel,
            uint targetCompressionLevel,
            uint expectedCompressionLevel,
            long origSize,
            IList<long> compressedSizes)
        {
            CreateTarget(minCompressionLevel, uint.MaxValue);

            var expectedResult = new Range<uint>(expectedCompressionLevel);

            Setup_OriginalFileSize(origSize);
            SetupReturnSequence_EncodedSize(compressedSizes);

            var result = target.InferCompressionLevel(new FileInfo("x"), targetCompressionLevel);

            Assert.AreEqual(expectedResult.MaxValue, result.MaxValue, "Max value");
            Assert.AreEqual(expectedResult.MinValue, result.MinValue, "Min value");
        }

        [TestCaseSource(nameof(Get_WhereFileIsSmallerThanTheTargetCompressionLevel))]
        public void When_TargetCompressionLevelIs_LowerThanOriginal__Should_KeepGettingRecodedFileSize_Until_CompressionLevelIsFound(
            uint maxCompressionLevel,
            uint targetCompressionLevel,
            uint expectedCompressionLevel,
            long origSize,
            IList<long> compressedSizes)
        {
            CreateTarget(uint.MinValue, maxCompressionLevel);

            var expectedResult = new Range<uint>(expectedCompressionLevel);

            Setup_OriginalFileSize(origSize);
            SetupReturnSequence_EncodedSize(compressedSizes);

            var result = target.InferCompressionLevel(new FileInfo("x"), targetCompressionLevel);

            Assert.AreEqual(expectedResult.MaxValue, result.MaxValue, "Max value");
            Assert.AreEqual(expectedResult.MinValue, result.MinValue, "Min value");
        }

        // todo: when file is still smaller

        [TestCaseSource(nameof(Get_WhereFileIsSmallerThanTheTargetCompressionLevel_NoExactCompressionLevel))]
        public void When_CantGetTheExactSameCompressedFileSize_And_TargetCompressionLevelIs_LowerThanOriginal__Should_Return_ARangeThatHasClosestCompressionLevelMatches(
            uint maxCompressionLevel,
            uint targetCompressionLevel,
            long origSize,
            uint expectedMinLevel,
            uint expectedMaxLevel,
            IList<long> compressedSizes)
        {
            CreateTarget(uint.MinValue, maxCompressionLevel);

            var expectedResult = new Range<uint>(expectedMinLevel, expectedMaxLevel);

            Setup_OriginalFileSize(origSize);
            SetupReturnSequence_EncodedSize(compressedSizes);

            var result = target.InferCompressionLevel(new FileInfo("x"), targetCompressionLevel);

            Assert.AreEqual(expectedResult.MaxValue, result.MaxValue, "Max value");
            Assert.AreEqual(expectedResult.MinValue, result.MinValue, "Min value");
        }

        [Test]
        [Ignore("todo: for this functionality, the type of compression level should be changed to int")]
        public void When_OriginalFileIsSmallerThanTheHighestCompressionRate__Should_Return_Minus1(
            uint targetCompressionLevel,
            long origSize,
            IList<long> compressedSizes)
        {
            CreateTarget(uint.MinValue, 0);

            Setup_OriginalFileSize(origSize);
            SetupReturnSequence_EncodedSize(compressedSizes);

            var result = target.InferCompressionLevel(new FileInfo("x"), targetCompressionLevel);

            Assert.AreEqual(-1, result.MaxValue, "Max value");
            Assert.AreEqual(-1, result.MinValue, "Min value");
        }

        [TestCaseSource(nameof(Get_WhereFileIsBiggerThanTheTargetCompressionLevel_NoExactCompressionLevel))]
        public void When_CantGetTheExactSameCompressedFileSize_And_TargetCompressionLevelIs_GreaterThanOriginal__Should_Return_ARangeThatHasClosestCompressionLevelMatches(
            uint minCompressionLevel,
            uint targetCompressionLevel,
            long origSize,
            uint expectedMinLevel,
            uint expectedMaxLevel,
            IList<long> compressedSizes)
        {
            CreateTarget(minCompressionLevel, uint.MaxValue);

            var expectedResult = new Range<uint>(expectedMinLevel, expectedMaxLevel);

            Setup_OriginalFileSize(origSize);
            SetupReturnSequence_EncodedSize(compressedSizes);

            var result = target.InferCompressionLevel(new FileInfo("x"), targetCompressionLevel);

            Assert.AreEqual(expectedResult.MaxValue, result.MaxValue, "Max value");
            Assert.AreEqual(expectedResult.MinValue, result.MinValue, "Min value");
        }

        private static IEnumerable<TestCaseData> GetHappyCases()
        {
            yield return new TestCaseData((uint)1, 100);
            yield return new TestCaseData((uint)0, 50);
            yield return new TestCaseData((uint)2, 150);
            yield return new TestCaseData((uint)10, 1000);
        }

        private static IEnumerable<TestCaseData> Get_WhereFileIsBiggerThanTheTargetCompressionLevel()
        {
            yield return new TestCaseData(
                (uint)0, //min
                (uint)10,//target
                (uint)9, //expected
                100,
                new long[] { 90, 100, 200, 300 }) //don't need any more numbers because the test would fail anyway
                .SetDescription("2nd attempt is successful");

            yield return new TestCaseData(
                (uint)5, //min
                (uint)10,//target
                (uint)9, //expected
                400,
                new long[] { 300, 400, 500, 600 }) //don't need any more numbers because the test would fail anyway
                .SetDescription("2nd attempt is successful");

            yield return new TestCaseData(
                (uint)5, //min
                (uint)10,//target
                (uint)8, //expected
                300,
                new long[] { 100, 200, 300, 400, 500 }) //don't need any more numbers because the test would fail anyway
                .SetDescription("3rd attempt is successful");

            yield return new TestCaseData(
                (uint)5,//min
                (uint)7,//target
                (uint)5,//expected
                300,
                new long[] { 100, 200, 300 }) //last number represents lowest compression level
                .SetDescription("Last attempt is successful");

            yield return new TestCaseData(
                (uint)1, //min
                (uint)2, //target
                (uint)1, //expected
                5,
                new long[] { 2, 5 }) //last number represents lowest compression level
                .SetDescription("Last attempt is successful");
        }

        private static IEnumerable<TestCaseData> Get_WhereFileIsSmallerThanTheTargetCompressionLevel()
        {
            yield return new TestCaseData(
                (uint)8, //max
                (uint)2, //target
                (uint)3, //expected
                100,
                new long[] { 200, 100, 50, 40 }) //don't need any more numbers because the test would fail anyway
                .SetDescription("2nd attempt is successful");

            yield return new TestCaseData(
                (uint)8, //max
                (uint)5, //target
                (uint)6, //expected
                400,
                new long[] { 500, 400, 300, 200 })
                .SetDescription("2nd attempt is successful");

            yield return new TestCaseData(
                (uint)8, //max
                (uint)4, //target
                (uint)6, //expected
                300,
                new long[] { 500, 400, 300, 200, 100 })
                .SetDescription("3rd attempt is successful");

            yield return new TestCaseData(
                (uint)7, //max
                (uint)5, //target
                (uint)7, //expected
                300,
                new long[] { 500, 400, 300 }) //last number represents highest compression level
                .SetDescription("Last attempt is successful");

            yield return new TestCaseData(
                (uint)8, //max
                (uint)7, //target
                (uint)8, //expected
                50,
                new long[] { 100, 50 }) //last number represents highest compression level
                .SetDescription("Last attempt is successful");
        }

        private static IEnumerable<TestCaseData> Get_WhereFileIsBiggerThanTheTargetCompressionLevel_NoExactCompressionLevel()
        {
            yield return new TestCaseData(
                uint.MinValue,
                (uint)3, //target level
                51, //file size
                (uint)2, (uint)3, //min/max result
                new long[] { 50, 100, 150, 200 })
                .SetDescription("between 1st and 2nd");

            yield return new TestCaseData(
                uint.MinValue,
                (uint)4, //target level
                399, //file size
                (uint)2, (uint)3, //min/max result
                new long[] { 200, 300, 400, 500, 600 })
                .SetDescription("between 2nd and 3rd");

            yield return new TestCaseData(
                (uint)0, //min level
                (uint)5, //target
                610,
                (uint)0, (uint)1, //min/max result
                new long[] { 100, 200, 300, 400, 500, 600 }) //last number represents highest compression level
                .SetDescription("between last and one before that");
        }

        private static IEnumerable<TestCaseData> Get_WhereFileIsSmallerThanTheTargetCompressionLevel_NoExactCompressionLevel()
        {
            yield return new TestCaseData(
                uint.MaxValue,
                (uint)4, //target
                399, //file size
                (uint)4, (uint)5, //min/max result
                new long[] { 400, 300, 200, 100, 50 })
                .SetDescription("between 1st and 2nd");

            yield return new TestCaseData(
                uint.MaxValue,
                (uint)8, //target
                699, //file size
                (uint)9, (uint)10, //min/max result
                new long[] { 800, 700, 600, 500, 400 })
                .SetDescription("between 2nd and 3rd");

            yield return new TestCaseData(
                uint.MaxValue,
                (uint)4, //target
                399, //file size
                (uint)6, (uint)7, //min/max result
                new long[] { 600, 500, 400, 300, 100 })
                .SetDescription("between 3rd and 4th");

            yield return new TestCaseData(
                (uint)10,
                (uint)7, //target
                50, //file size
                (uint)9, (uint)10, //min/max result
                new long[] { 600, 500, 100, 10 })
                .SetDescription("between last and last to last");
        }


        private void Setup_OriginalFileSize(long returnValue)
        {
            fileSizeGetter
                .Setup(
                    x => x.GetSize(
                        It.IsAny<FileInfo>()))
                .Returns(returnValue);
        }

        private void Setup_EncodedSize(long returnValue)
        {
            encoder
                .Setup(
                    x => x.GetCompressedSize(
                        It.IsAny<FileInfo>(),
                        It.IsAny<uint>()))
                .Returns(returnValue);
        }

        private void SetupReturnSequence_EncodedSize(IEnumerable<long> returnValues)
        {
            var setup = encoder
                .SetupSequence(
                    x => x.GetCompressedSize(
                        It.IsAny<FileInfo>(),
                        It.IsAny<uint>()));

            foreach (var resultSize in returnValues)
                setup = setup.Returns(resultSize);

            //make sure that it doesn't succeed when attempting to go beyond encoder's supported compression level
            setup.Throws(new Exception("Compression Level is not supported"));
        }

        public struct Range
        {
            public uint Min { get; set; }
            public uint Max { get; set; }
        }
    }
}