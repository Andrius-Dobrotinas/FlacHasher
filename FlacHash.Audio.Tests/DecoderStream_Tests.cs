using Andy.ExternalProcess;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

            result.Should().Be(expectedPosition, "Resulting position");
            source.Position.Should().Be(expectedPosition, "Source stream position");
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

            result.Should().Be(expectedPosition, "Resulting position");
            source.Position.Should().Be(expectedPosition, "Source stream position");
        }

        [TestCaseSource(nameof(GetExpectedExceptionMapping))]
        public void Seek__When_TheSourceStream_Throws__Must_Wrap_It_In_TheCorresponding_Exception(Exception thrownBySource, Type expectedExceptionType)
        {
            var target = Stream_Throwing_On_Seek(thrownBySource);

            Assert_Wrapped(thrownBySource, expectedExceptionType, () => target.Seek(0, SeekOrigin.Begin));
        }

        [TestCaseSource(nameof(GetRethrownExceptions))]
        public void Seek__When_TheSourceStream_Throws_A_Cancellation_Exception__Must_Rethrow_It(Exception thrownBySource)
        {
            var target = Stream_Throwing_On_Seek(thrownBySource);

            Assert_Rethrown(thrownBySource, () => target.Seek(0, SeekOrigin.Begin));
        }

        [TestCaseSource(nameof(Read_Cases))]
        public void Read__Must_Read_TheSourceStream_Into_TheSpecified_Segment_Of_TheBuffer(int bufferSize, int offset, int count, int expectedResult, byte[] expectedBuffer)
        {
            var source = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var target = new DecoderStream(source);
            var buffer = new byte[bufferSize];

            var result = target.Read(buffer, offset, count);

            result.Should().Be(expectedResult, "Number of bytes read");
            buffer.Should().Equal(expectedBuffer, "Buffer contents");
        }

        [Test]
        public void Read__Must_Continue_From_TheCurrent_Position_Of_TheSourceStream()
        {
            var source = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var target = new DecoderStream(source);
            var buffer = new byte[2];

            target.Read(buffer, 0, 2);
            var result = target.Read(buffer, 0, 2);

            result.Should().Be(2, "Number of bytes read");
            buffer.Should().Equal(new byte[] { 3, 4 }, "Buffer contents");
        }

        [Test]
        public void Read__At_TheEnd_Of_TheSourceStream__Must_Return_Zero()
        {
            var source = new MemoryStream(new byte[] { 1, 2, 3 });
            source.Position = 3;
            var target = new DecoderStream(source);

            var result = target.Read(new byte[3], 0, 3);

            result.Should().Be(0);
        }

        [Test]
        public void Read__Must_Pass_TheArguments_To_TheSourceStream_And_Return_ItsResult()
        {
            var buffer = new byte[10];
            var source = new Mock<Stream>();
            source.Setup(x => x.Read(buffer, 2, 5)).Returns(7);
            var target = new DecoderStream(source.Object);

            var result = target.Read(buffer, 2, 5);

            result.Should().Be(7, "Number of bytes read");
            source.Verify(x => x.Read(buffer, 2, 5), Times.Once);
        }

        [TestCaseSource(nameof(GetExpectedExceptionMapping))]
        public void Read__When_TheSourceStream_Throws__Must_Wrap_It_In_TheCorresponding_Exception(Exception thrownBySource, Type expectedExceptionType)
        {
            var target = Stream_Throwing_On_Read(thrownBySource);

            Assert_Wrapped(thrownBySource, expectedExceptionType, () => target.Read(new byte[1], 0, 1));
        }

        [TestCaseSource(nameof(GetRethrownExceptions))]
        public void Read__When_TheSourceStream_Throws_A_Cancellation_Exception__Must_Rethrow_It(Exception thrownBySource)
        {
            var target = Stream_Throwing_On_Read(thrownBySource);

            Assert_Rethrown(thrownBySource, () => target.Read(new byte[1], 0, 1));
        }

        [Test]
        public void Dispose__Must_Dispose_TheSourceStream()
        {
            var source = new Mock<Stream>();
            var target = new DecoderStream(source.Object);

            target.Dispose();

            source.Verify(x => x.Close(), Times.Once);
        }

        [Test]
        public void Dispose__Called_More_Than_Once__Must_Not_Throw()
        {
            var target = new DecoderStream(new MemoryStream(new byte[5]));

            target.Dispose();

            Action act = () => target.Dispose();
            act.Should().NotThrow();
        }

        [TestCaseSource(nameof(GetExpectedExceptionMapping))]
        public void Dispose__When_TheSourceStream_Throws__Must_Wrap_It_In_TheCorresponding_Exception(Exception thrownBySource, Type expectedExceptionType)
        {
            var target = Stream_Throwing_On_Dispose(thrownBySource);

            Assert_Wrapped(thrownBySource, expectedExceptionType, () => target.Dispose());
        }

        [TestCaseSource(nameof(GetRethrownExceptions))]
        public void Dispose__When_TheSourceStream_Throws_A_Cancellation_Exception__Must_Rethrow_It(Exception thrownBySource)
        {
            var target = Stream_Throwing_On_Dispose(thrownBySource);

            Assert_Rethrown(thrownBySource, () => target.Dispose());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CanRead__Must_Return_TheValue_Of_TheSourceStream(bool sourceValue)
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.CanRead).Returns(sourceValue);
            var target = new DecoderStream(source.Object);

            target.CanRead.Should().Be(sourceValue);
        }

        [TestCaseSource(nameof(GetExpectedExceptionMapping))]
        public void CanRead__When_TheSourceStream_Throws__Must_Wrap_It_In_TheCorresponding_Exception(Exception thrownBySource, Type expectedExceptionType)
        {
            var target = Stream_Throwing_On_CanRead(thrownBySource);

            Assert_Wrapped(thrownBySource, expectedExceptionType, () => { _ = target.CanRead; });
        }

        [TestCaseSource(nameof(GetRethrownExceptions))]
        public void CanRead__When_TheSourceStream_Throws_A_Cancellation_Exception__Must_Rethrow_It(Exception thrownBySource)
        {
            var target = Stream_Throwing_On_CanRead(thrownBySource);

            Assert_Rethrown(thrownBySource, () => { _ = target.CanRead; });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CanSeek__Must_Return_TheValue_Of_TheSourceStream(bool sourceValue)
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.CanSeek).Returns(sourceValue);
            var target = new DecoderStream(source.Object);

            target.CanSeek.Should().Be(sourceValue);
        }

        [TestCaseSource(nameof(GetExpectedExceptionMapping))]
        public void CanSeek__When_TheSourceStream_Throws__Must_Wrap_It_In_TheCorresponding_Exception(Exception thrownBySource, Type expectedExceptionType)
        {
            var target = Stream_Throwing_On_CanSeek(thrownBySource);

            Assert_Wrapped(thrownBySource, expectedExceptionType, () => { _ = target.CanSeek; });
        }

        [TestCaseSource(nameof(GetRethrownExceptions))]
        public void CanSeek__When_TheSourceStream_Throws_A_Cancellation_Exception__Must_Rethrow_It(Exception thrownBySource)
        {
            var target = Stream_Throwing_On_CanSeek(thrownBySource);

            Assert_Rethrown(thrownBySource, () => { _ = target.CanSeek; });
        }

        [Test]
        public void CanWrite__Must_Be_False__Even_When_TheSourceStream_Is_Writable()
        {
            var source = new MemoryStream();
            Assume.That(source.CanWrite, Is.True);
            var target = new DecoderStream(source);

            target.CanWrite.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(10)]
        public void Length__Must_Return_TheLength_Of_TheSourceStream(int length)
        {
            var target = new DecoderStream(new MemoryStream(new byte[length]));

            target.Length.Should().Be(length);
        }

        [TestCaseSource(nameof(GetExpectedExceptionMapping))]
        public void Length__When_TheSourceStream_Throws__Must_Wrap_It_In_TheCorresponding_Exception(Exception thrownBySource, Type expectedExceptionType)
        {
            var target = Stream_Throwing_On_Length(thrownBySource);

            Assert_Wrapped(thrownBySource, expectedExceptionType, () => { _ = target.Length; });
        }

        [TestCaseSource(nameof(GetRethrownExceptions))]
        public void Length__When_TheSourceStream_Throws_A_Cancellation_Exception__Must_Rethrow_It(Exception thrownBySource)
        {
            var target = Stream_Throwing_On_Length(thrownBySource);

            Assert_Rethrown(thrownBySource, () => { _ = target.Length; });
        }

        [TestCase(0)]
        [TestCase(5)]
        [TestCase(10)]
        public void Position__Must_Return_ThePosition_Of_TheSourceStream(int position)
        {
            var source = new MemoryStream(new byte[10]);
            source.Position = position;
            var target = new DecoderStream(source);

            target.Position.Should().Be(position);
        }

        [TestCase(0)]
        [TestCase(5)]
        [TestCase(10)]
        public void Position_Set__Must_Set_ThePosition_Of_TheSourceStream(int position)
        {
            var source = new MemoryStream(new byte[10]);
            var target = new DecoderStream(source);

            target.Position = position;

            source.Position.Should().Be(position, "Source stream position");
            target.Position.Should().Be(position, "Resulting position");
        }

        [TestCaseSource(nameof(GetExpectedExceptionMapping))]
        public void Position__When_TheSourceStream_Throws__Must_Wrap_It_In_TheCorresponding_Exception(Exception thrownBySource, Type expectedExceptionType)
        {
            var target = Stream_Throwing_On_Position_Get(thrownBySource);

            Assert_Wrapped(thrownBySource, expectedExceptionType, () => { _ = target.Position; });
        }

        [TestCaseSource(nameof(GetRethrownExceptions))]
        public void Position__When_TheSourceStream_Throws_A_Cancellation_Exception__Must_Rethrow_It(Exception thrownBySource)
        {
            var target = Stream_Throwing_On_Position_Get(thrownBySource);

            Assert_Rethrown(thrownBySource, () => { _ = target.Position; });
        }

        [TestCaseSource(nameof(GetExpectedExceptionMapping))]
        public void Position_Set__When_TheSourceStream_Throws__Must_Wrap_It_In_TheCorresponding_Exception(Exception thrownBySource, Type expectedExceptionType)
        {
            var target = Stream_Throwing_On_Position_Set(thrownBySource);

            Assert_Wrapped(thrownBySource, expectedExceptionType, () => target.Position = 5);
        }

        [TestCaseSource(nameof(GetRethrownExceptions))]
        public void Position_Set__When_TheSourceStream_Throws_A_Cancellation_Exception__Must_Rethrow_It(Exception thrownBySource)
        {
            var target = Stream_Throwing_On_Position_Set(thrownBySource);

            Assert_Rethrown(thrownBySource, () => target.Position = 5);
        }

        [Test]
        public void SetLength__Must_Throw_NotImplementedException_And_Not_Touch_TheSourceStream()
        {
            var source = new Mock<Stream>();
            var target = new DecoderStream(source.Object);

            Action act = () => target.SetLength(5);
            act.Should().Throw<NotImplementedException>();
            source.VerifyNoOtherCalls();
        }

        [Test]
        public void Write__Must_Throw_NotImplementedException_And_Not_Touch_TheSourceStream()
        {
            var source = new Mock<Stream>();
            var target = new DecoderStream(source.Object);

            Action act = () => target.Write(new byte[5], 0, 5);
            act.Should().Throw<NotImplementedException>();
            source.VerifyNoOtherCalls();
        }

        [Test]
        public void Flush__Must_Throw_NotImplementedException_And_Not_Touch_TheSourceStream()
        {
            var source = new Mock<Stream>();
            var target = new DecoderStream(source.Object);

            Action act = () => target.Flush();
            act.Should().Throw<NotImplementedException>();
            source.VerifyNoOtherCalls();
        }

        static void Assert_Wrapped(Exception thrownBySource, Type expectedExceptionType, Action action)
        {
            var actual = action.Should().Throw<Exception>().Which;

            actual.Should().BeOfType(expectedExceptionType);
            actual.InnerException.Should().BeSameAs(thrownBySource, nameof(Exception.InnerException));

            if (actual is DecoderException decoderException)
                decoderException.ActualException.Should().BeSameAs(thrownBySource, nameof(DecoderException.ActualException));
        }

        static void Assert_Rethrown(Exception thrownBySource, Action action)
        {
            var actual = action.Should().Throw<Exception>().Which;

            actual.Should().BeSameAs(thrownBySource, "Must rethrow the original exception as is");
        }

        static DecoderStream Stream_Throwing_On_Read(Exception exception)
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(exception);
            return new DecoderStream(source.Object);
        }

        static DecoderStream Stream_Throwing_On_Seek(Exception exception)
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.Seek(It.IsAny<long>(), It.IsAny<SeekOrigin>())).Throws(exception);
            return new DecoderStream(source.Object);
        }

        static DecoderStream Stream_Throwing_On_Dispose(Exception exception)
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.Close()).Throws(exception);
            return new DecoderStream(source.Object);
        }

        static DecoderStream Stream_Throwing_On_CanRead(Exception exception)
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.CanRead).Throws(exception);
            return new DecoderStream(source.Object);
        }

        static DecoderStream Stream_Throwing_On_CanSeek(Exception exception)
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.CanSeek).Throws(exception);
            return new DecoderStream(source.Object);
        }

        static DecoderStream Stream_Throwing_On_Length(Exception exception)
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.Length).Throws(exception);
            return new DecoderStream(source.Object);
        }

        static DecoderStream Stream_Throwing_On_Position_Get(Exception exception)
        {
            var source = new Mock<Stream>();
            source.Setup(x => x.Position).Throws(exception);
            return new DecoderStream(source.Object);
        }

        static DecoderStream Stream_Throwing_On_Position_Set(Exception exception)
        {
            var source = new Mock<Stream>();
            source.SetupSet(x => x.Position = It.IsAny<long>()).Throws(exception);
            return new DecoderStream(source.Object);
        }

        static IEnumerable<TestCaseData> GetExpectedExceptionMapping()
        {
            yield return ExceptionCase(new ExecutionException(-1), typeof(DecoderException), "Without_ProcessErrorOutput");
            yield return ExceptionCase(new ExecutionException(2, "decoder error output"), typeof(DecoderException), "With_ProcessErrorOutput");
            yield return ExceptionCase(new System.IO.IOException("Failure"), typeof(GenericDecoderException));
            yield return ExceptionCase(new ObjectDisposedException("source"), typeof(GenericDecoderException));
            yield return ExceptionCase(new NotSupportedException(), typeof(GenericDecoderException));
        }

        static IEnumerable<TestCaseData> GetRethrownExceptions()
        {
            yield return RethrownCase(new OperationCanceledException());
            // A subclass of OperationCanceledException: proves the catch-clause order doesn't wrap cancellation
            yield return RethrownCase(new TaskCanceledException());
        }

        static TestCaseData ExceptionCase(Exception thrownBySource, Type expectedExceptionType, string distinguisher = null)
        {
            var name = thrownBySource.GetType().Name + (distinguisher == null ? null : $"_{distinguisher}");
            return new TestCaseData(thrownBySource, expectedExceptionType)
                .SetName($"{{m}}__{name}__{expectedExceptionType.Name}");
        }

        static TestCaseData RethrownCase(Exception thrownBySource)
        {
            return new TestCaseData(thrownBySource)
                .SetName($"{{m}}__{thrownBySource.GetType().Name}");
        }

        static IEnumerable<TestCaseData> Read_Cases()
        {
            yield return new TestCaseData(5, 0, 5, 5, new byte[] { 1, 2, 3, 4, 5 });
            yield return new TestCaseData(5, 0, 3, 3, new byte[] { 1, 2, 3, 0, 0 });
            yield return new TestCaseData(5, 2, 3, 3, new byte[] { 0, 0, 1, 2, 3 });
            yield return new TestCaseData(5, 0, 0, 0, new byte[] { 0, 0, 0, 0, 0 });
            yield return new TestCaseData(8, 0, 8, 5, new byte[] { 1, 2, 3, 4, 5, 0, 0, 0 });
        }
    }
}
