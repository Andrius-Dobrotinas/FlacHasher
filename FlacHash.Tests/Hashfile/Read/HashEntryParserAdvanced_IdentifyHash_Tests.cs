using NUnit.Framework;

namespace Andy.FlacHash.Hashfile.Read
{
    public class HashEntryParserAdvanced_IdentifyHash_Tests
    {
        readonly HashEntryParserAdvanced target = new HashEntryParserAdvanced();

        static KeyValuePair<string, string> RequireResult(KeyValuePair<string, string>? result)
        {
            Assert.IsNotNull(result);
            return result.Value;
        }

        [TestCase("DEADBEEFDEADBEEF")]
        [TestCase("deadbeefdeadbeef")]
        [TestCase("11223344AABBCCDD")]
        [TestCase("8c6c0210e16e3853ff1bd8eb52917243e2706fc5057692d0f560f066045523f6")]
        [TestCase("8C6C0210E16E3853FF1BD8EB52917243E2706FC5057692D0F560F066045523F6")]
        public void Must_Be_HexString_AtLeast_8_Bytes_Long_CaseInsentive(string input)
        {
            var result = RequireResult(target.Parse(input));

            Assert.AreEqual(input, result.Value, "Hash");
        }

        [TestCase("[DEADBEAF00112233]", "DEADBEAF00112233")]
        [TestCase("(DEADBEAF00112233)", "DEADBEAF00112233")]
        [TestCase("{DEADBEAF00112233}", "DEADBEAF00112233")]
        [TestCase("`DEADBEAF00112233`", "DEADBEAF00112233")]
        public void Must_Accept_Hash_Wrapped_In_Brackets__HashOnly(string input, string hash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.IsNull(result.Key, "Filename");
            Assert.AreEqual(hash, result.Value, "Hash");
        }

        [TestCase("DEADBEEFDEADBEEF,")]
        [TestCase("\"DEADBEEFDEADBEEF\"")]
        [TestCase("foo.flac-DEADBEEFDEADBEEF")]
        [TestCase("song_DEADBEEF00112233.flac")]
        [TestCase("DEADBEEF00112233.flac")]
        public void MustNot_Be_PrefixedOrSuffixedBy_AnythingThanAllowed_Chars(string input)
        {
            Assert.Throws<MissingHashValueException>(() => target.Parse(input));
        }

        [TestCase("DE")]
        [TestCase("deadbe")]
        [TestCase("DEADBEF")]
        [TestCase("DEADBEEF")]
        [TestCase("DEADBEEFEFEFEF")]
        public void MustNot_Be_LessThan_8_Bytes(string input)
        {
            Assert.Throws<MissingHashValueException>(() => target.Parse(input));
        }

        [TestCase("DEADBEEX")]
        [TestCase("11-22-33-44-AA-BB-CC-DD")]
        [TestCase("XEADBEAF00112233")]
        public void MustNot_Contain_NonHex_Chars(string input)
        {
            Assert.Throws<MissingHashValueException>(() => target.Parse(input));
        }
    }
}
