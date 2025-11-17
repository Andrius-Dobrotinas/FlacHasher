using Andy.FlacHash;
using NUnit.Framework;
using System;

namespace Andy.FlacHash
{
    public class HashFormatting_Tests
    {
        [TestCase(new byte[] { 0x01, 0xAB, 0xFF }, "01abff")]
        [TestCase(new byte[] { 0x10, 0x20 }, "1020")]
        [TestCase(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, "deadbeef")]
        public void Must_Return_LowercaseHexString_WithoutSeparators(byte[] hash, string expected)
        {
            var formatted = HashFormatting.GetInLowercase(hash);

            Assert.AreEqual(expected, formatted);
        }

        [Test]
        public void When_HashIsEmpty__Must_Return_EmptyString()
        {
            var hash = Array.Empty<byte>();

            var formatted = HashFormatting.GetInLowercase(hash);

            Assert.AreEqual(string.Empty, formatted);
        }

        [Test]
        public void When_HashIsNull__Must_Throw_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => HashFormatting.GetInLowercase(null));
        }
    }
}
