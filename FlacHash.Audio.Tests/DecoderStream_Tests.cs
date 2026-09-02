using Andy.ExternalProcess;
using Moq;
using NUnit.Framework;
using System;
using System.IO;

namespace Andy.FlacHash.Audio
{
    public class DecoderStream_Tests
    {
        /// <summary>
        /// If Seek ever recurses into itself, these tests kill the test host with an uncatchable StackOverflowException instead of failing.
        /// </summary>
        [TestCase(0, SeekOrigin.Begin, 0)]
        [TestCase(3, SeekOrigin.Begin, 3)]
        [TestCase(10, SeekOrigin.Begin, 10)]
        [TestCase(-4, SeekOrigin.End, 6)]
        public void Seek__Must_Seek_The_Source_Stream(int offset, SeekOrigin origin, long expectedPosition)
        {
            var source = new MemoryStream(new byte[10]);
            var target = new DecoderStream(source);

            var result = target.Seek(offset, origin);

            Assert.AreEqual(expectedPosition, result, "Resulting position");
            Assert.AreEqual(expectedPosition, source.Position, "Source stream position");
        }

        /// <summary>
        /// Seeking by <see cref="SeekOrigin.Current"/> is only distinguishable from <see cref="SeekOrigin.Begin"/> when the source stream isn't at position zero.
        /// </summary>
        [TestCase(5, 2, 7)]
        [TestCase(5, -3, 2)]
        [TestCase(3, 0, 3)]
        public void Seek__Relative_To_Current_Position__Must_Seek_The_Source_Stream(int initialPosition, int offset, long expectedPosition)
        {
            var source = new MemoryStream(new byte[10]);
            source.Position = initialPosition;
            var target = new DecoderStream(source);

            var result = target.Seek(offset, SeekOrigin.Current);

            Assert.AreEqual(expectedPosition, result, "Resulting position");
            Assert.AreEqual(expectedPosition, source.Position, "Source stream position");
        }

        [Test]
        public void Seek__When_TheSourceStream_Throws_ExecutionException__Must_Throw_DecoderException()
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.Seek(It.IsAny<long>(), It.IsAny<SeekOrigin>()))
                .Throws(new ExecutionException(-1));
            var target = new DecoderStream(source.Object);

            Assert.Throws<DecoderException>(() => target.Seek(0, SeekOrigin.Begin));
        }
    }
}
