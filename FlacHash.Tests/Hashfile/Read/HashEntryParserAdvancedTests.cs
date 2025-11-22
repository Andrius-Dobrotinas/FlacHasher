using NUnit.Framework;

namespace Andy.FlacHash.Hashfile.Read
{
    public class HashEntryParserAdvancedTests
    {
        readonly HashEntryParserAdvanced target = new HashEntryParserAdvanced();

        static KeyValuePair<string, string> RequireResult(KeyValuePair<string, string>? result)
        {
            Assert.IsNotNull(result);
            return result.Value;
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        [TestCase("\t")]
        [TestCase(" \t  ")]
        public void When_EmptyOrWhitespace_ReturnsNull(string input)
        {
            var result = target.Parse(input);

            Assert.IsNull(result);
        }

        [TestCase("DE")]
        [TestCase("deadbe")]
        [TestCase("DEADBEF")]
        public void IdentifyHash_When_HashTooShort__ReturnNull(string input)
        {
            var result = target.Parse(input);

            Assert.IsNull(result);
        }

        [TestCase("DEADBEEX")]
        [TestCase("11-22-33-44-AA-BB-CC-DD")]
        [TestCase("XEADBEAF00112233")]
        public void IdentifyHash_When_HashIsNotHex__ReturnNull(string input)
        {
            var result = target.Parse(input);

            Assert.IsNull(result);
        }

        [TestCase("DEADBEEF")]
        [TestCase("deadbeef")]
        [TestCase("11223344AABBCCDD")]
        [TestCase("8c6c0210e16e3853ff1bd8eb52917243e2706fc5057692d0f560f066045523f6")]
        [TestCase("8C6C0210E16E3853FF1BD8EB52917243E2706FC5057692D0F560F066045523F6")]
        public void IdentifyHash_MustBe_HexString_AtLeast8CharsLong_CaseInsentive(string input)
        {
            var result = RequireResult(target.Parse(input));

            Assert.AreEqual(input, result.Value, "Hash");
        }

        [TestCase("DEADBEEF,")]
        [TestCase("[DEADBEEF]")]
        [TestCase("foo.flac-DEADBEEF")]
        [TestCase("song_DEADBEEF00112233.flac")]
        [TestCase("DEADBEEF00112233.flac")]
        public void IdentifyHash_When_HashBoundaryIs_NonWordChars_ReturnNull(string input)
        {
            var result = target.Parse(input);

            Assert.IsNull(result);
        }

        [TestCase("slts.flac")]
        [TestCase("slts")]
        [TestCase("Smells Like Teen Spirit")]
        [TestCase("Nirvana : in bloom.flac")]
        [TestCase("Nirvana + Melvins > Melvana")]
        [TestCase(@"Nirvana - ""Lithium"".wav")]
        [TestCase("01 - Smells Like Teen Spirit")]
        [TestCase("01 - Smells Like Teen Spirit.flac")]
        [TestCase("01. Smells Like Teen Spirit")]
        [TestCase("[01] Smells Like Teen Spirit.flac")]
        public void When_NoHash_JustFilename_ReturnsNull(string input)
        {
            var result = target.Parse(input);

            Assert.IsNull(result);
        }

        [TestCase("##  just text")]
        [TestCase("- slts.flac")]
        [TestCase("# [01] Smells Like Teen Spirit")]
        [TestCase("+ 01 - Smells Like Teen Spirit.flac")]
        [TestCase("--\tNirvana : in bloom.flac")]
        public void When_NoHash_WithPrefix_ReturnsNull(string input)
        {
            var result = target.Parse(input);
            
            Assert.IsNull(result);
        }

        [TestCase("DEADBEAF00112233", "DEADBEAF00112233")]
        [TestCase("  deadbeef00112233  ", "deadbeef00112233")]
        [TestCase("\tDEADbeef00112233", "DEADbeef00112233")]
        [TestCase("\tDEADbeef00112233 ", "DEADbeef00112233")]
        public void ExtractSegments_HashOnly_Returns_TheHash(string input, string expectedHash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.IsNull(result.Key, "FileName");
            Assert.AreEqual(expectedHash, result.Value, "Hash");
        }

        [TestCase(" DEADBEF ")]
        [TestCase("slts.flac DEADBEF")]
        [TestCase("DEADBEF slts.flac")]
        [TestCase("# DEADBEF slts.flac")]
        [TestCase("01 - Smells Like Teen Spirit.flac 11-22-33-44-AA-BB-CC-DD")]
        [TestCase("# 11-22-33-44-AA-BB-CC-DD irrelevant text")]
        public void ExtractSegments_When_Line_ContainsInvalidHash__ReturnNull(string input)
        {
            var result = target.Parse(input);

            Assert.IsNull(result);
        }

        [TestCase(" ")]
        [TestCase("  ")]
        [TestCase("\t")]
        [TestCase(" \t")]
        [TestCase(" - ")]
        [TestCase(" - ")]
        public void ExtractSegments_When_Separators_AreWhitespace__FileFirst(string separator)
        {
            var result = RequireResult(target.Parse($"slts.flac{separator}DEADBEAF00112233"));

            Assert.AreEqual("slts.flac", result.Key, "FileName");
            Assert.AreEqual("DEADBEAF00112233", result.Value, "Hash");
        }

        [TestCase(" ")]
        [TestCase("  ")]
        [TestCase("\t")]
        [TestCase(" \t")]
        [TestCase(" - ")]
        [TestCase(" - ")]
        public void ExtractSegments_When_Separators_AreWhitespace__HashFirst(string separator)
        {
            var result = RequireResult(target.Parse($"DEADBEAF00112233{separator}slts.flac"));

            Assert.AreEqual("slts.flac", result.Key, "FileName");
            Assert.AreEqual("DEADBEAF00112233", result.Value, "Hash");
        }

        public static IEnumerable<TestCaseData> GetValidSeparators()
        {
            foreach (var separatorSequence in new string[]
            {
                ">", "<", "=", "-", "*", "|", "#",
                "--", "->", "=>", ">>", "||", "##",
                "<--->", "*-*", "-=*", "<>|<>", "=*-=*=", "#->"
            })
            {
                yield return new TestCaseData(separatorSequence);
            }
        }

        [TestCaseSource(nameof(GetValidSeparators))]
        public void ExtractSegments_FileFirst_When_Separators_SurroundedBySingleSpaces(string separator)
        {
            var result = RequireResult(target.Parse($"slts.flac {separator} DEADBEAF00112233"));

            Assert.AreEqual("slts.flac", result.Key, "FileName");
            Assert.AreEqual("DEADBEAF00112233", result.Value, "Hash");
        }

        [TestCaseSource(nameof(GetValidSeparators))]
        public void ExtractSegments_HashFirst_When_Separators_SurroundedBySingleSpaces(string separator)
        {
            var result = RequireResult(target.Parse($"DEADBEAF00112233 {separator} slts.flac"));

            Assert.AreEqual("slts.flac", result.Key, "FileName");
            Assert.AreEqual("DEADBEAF00112233", result.Value, "Hash");
        }

        [TestCaseSource(nameof(GetValidSeparators))]
        public void ExtractSegments_FileFirst_When_Separators_Are_SurroundedByVariousWhitespace(string separator)
        {
            var result = RequireResult(target.Parse($"slts.flac\t{separator}  DEADBEAF00112233"));

            Assert.AreEqual("slts.flac", result.Key, "FileName");
            Assert.AreEqual("DEADBEAF00112233", result.Value, "Hash");
        }

        [TestCaseSource(nameof(GetValidSeparators))]
        public void ExtractSegments_HashFirst_When_Separators_Are_SurroundedByVariousWhitespace(string separator)
        {
            var result = RequireResult(target.Parse($"DEADBEAF00112233\t{separator}  slts.flac"));

            Assert.AreEqual("slts.flac", result.Key, "FileName");
            Assert.AreEqual("DEADBEAF00112233", result.Value, "Hash");
        }

        [TestCaseSource(nameof(GetValidSeparators))]
        public void ExtractSegments_When_LineStartsWithPrefix(string separator)
        {
            var result = RequireResult(target.Parse($"#slts.flac\t{separator} DEADBEAF00112233"));

            Assert.AreEqual("slts.flac", result.Key, "FileName");
            Assert.AreEqual("DEADBEAF00112233", result.Value, "Hash");
        }

        [TestCase("slts.flac DEADBEAF00112233", "slts.flac", "DEADBEAF00112233")]
        [TestCase("slts DEADBEAF00112233", "slts", "DEADBEAF00112233")]
        [TestCase("Smells Like Teen Spirit DEADBEAF00112233", "Smells Like Teen Spirit", "DEADBEAF00112233")]
        [TestCase("01 - Smells Like Teen Spirit.fla DEADBEAF00112233", "01 - Smells Like Teen Spirit.fla", "DEADBEAF00112233")]
        [TestCase("01. Smells Like Teen Spirit.flac DEADBEAF00112233", "01. Smells Like Teen Spirit.flac", "DEADBEAF00112233")]
        [TestCase("[01] Smells Like Teen Spirit.flac DEADBEAF00112233", "[01] Smells Like Teen Spirit.flac", "DEADBEAF00112233")]
        [TestCase("Nirvana : in bloom.flac DEADBEAF00112233", "Nirvana : in bloom.flac", "DEADBEAF00112233")]
        [TestCase("Nirvana + Melvins > Melvana DEADBEAF00112233", "Nirvana + Melvins > Melvana", "DEADBEAF00112233")]
        [TestCase(@"Nirvana - ""Lithium"".wav DEADBEAF00112233", @"Nirvana - ""Lithium"".wav", "DEADBEAF00112233")]
        [TestCase("slts#1.flac DEADBEAF00112233", "slts#1.flac", "DEADBEAF00112233")]
        [TestCase("/home/user/music/slts.flac DEADBEAF00112233", "/home/user/music/slts.flac", "DEADBEAF00112233")]
        [TestCase("Nirvana - MV - DEADBEAF00112233", "Nirvana - MV", "DEADBEAF00112233")]
        public void ExtractSegments_FileFirst_Return_Filename(string input, string expectedFilename, string expectedHash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.AreEqual(expectedFilename, result.Key, "FileName");
            Assert.AreEqual(expectedHash, result.Value, "Hash");
        }

        [TestCase("slts.flac DEADBEAF00112233  extra text after hash", "slts.flac", "DEADBEAF00112233")]
        [TestCase("slts.flac -- DEADBEAF00112233 # this is not a comment", "slts.flac", "DEADBEAF00112233")]
        [TestCase("slts.flac - DEADBEAF00112233 -- caya.flac", "slts.flac", "DEADBEAF00112233")]
        public void ExtractSegments_FileFirst_Must_IgnoreTrailingText(string input, string expectedFilename, string expectedHash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.AreEqual(expectedFilename, result.Key, "FileName");
            Assert.AreEqual(expectedHash, result.Value, "Hash");
        }

        [TestCase("# DEADBEAF00112233", "DEADBEAF00112233")]
        [TestCase("#DEADBEAF00112233", "DEADBEAF00112233")]
        [TestCase("## DEADBEAF00112233", "DEADBEAF00112233")]
        [TestCase("##   DEADBEAF00112233", "DEADBEAF00112233")]
        [TestCase("--\tdeadbeef00112233", "deadbeef00112233")]
        [TestCase("***  DEADBEAF00112233   ", "DEADBEAF00112233")]
        public void ExtractSegments_HashOnly_When_LineStartsWithSpecialCharPrefix_IgnorePrefix(string input, string expectedHash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.IsNull(result.Key, "FileName");
            Assert.AreEqual(expectedHash, result.Value, "Hash");
        }

        public static IEnumerable<TestCaseData> GetValidPrefixes()
        {
            foreach (var separatorSequence in new string[] { "#", "-", "+", "*", "<", ">", "=", "##", "--", "++", "**", "<<", ">>", "==", "#*", "-+", "+-", "*#", "<>", "><", "=>" })
                yield return new TestCaseData(separatorSequence);
        }

        [TestCaseSource(nameof(GetValidPrefixes))]
        public void ExtractSegments_When_LineStartsWithSpecialCharPrefix_IgnorePrefix(string prefix)
        {
            var result = RequireResult(target.Parse($"{prefix} slts.flac  DEADBEAF00112233"));

            Assert.AreEqual("slts.flac", result.Key, "FileName");
            Assert.AreEqual("DEADBEAF00112233", result.Value, "Hash");
        }

        [TestCase("##  slts.flac  DEADBEAF00112233", "slts.flac", "DEADBEAF00112233")]
        [TestCase("--\tSmells Like Teen Spirit  DEADBEAF00112233", "Smells Like Teen Spirit", "DEADBEAF00112233")]
        [TestCase("++  DEADBEAF00112233  slts.flac", "slts.flac", "DEADBEAF00112233")]
        public void ExtractSegments_When_LineStartsWithSpecialCharPrefix_IgnorePrefix__VariousFileInputs(string input, string expectedFilename, string expectedHash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.AreEqual(expectedFilename, result.Key, "FileName");
            Assert.AreEqual(expectedHash, result.Value, "Hash");
        }

        [TestCase("slts.flac DEADBEEF", "slts.flac", "DEADBEEF")]
        [TestCase("slts.flac 8c6c0210e16e3853ff1bd8eb52917243e2706fc5057692d0f560f066045523f6", "slts.flac", "8c6c0210e16e3853ff1bd8eb52917243e2706fc5057692d0f560f066045523f6")]
        [TestCase("8c6c0210e16e3853ff1bd8eb52917243e2706fc5057692d0f560f066045523f6 slts.flac", "slts.flac", "8c6c0210e16e3853ff1bd8eb52917243e2706fc5057692d0f560f066045523f6")]
        public void ExtractSegments_ReturnHash(string input, string expectedFilename, string expectedHash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.AreEqual(expectedFilename, result.Key, "FileName");
            Assert.AreEqual(expectedHash, result.Value, "Hash");
        }

        [TestCase("DEADBEEF01.flac 11223344AABBCCDD", "DEADBEEF01.flac", "11223344AABBCCDD")]
        [TestCase("11223344AABBCCDD.flac DEADBEAF00112233", "11223344AABBCCDD.flac", "DEADBEAF00112233")]
        [TestCase("11223344aabbccdd.ape DEADBEAF00112233", "11223344aabbccdd.ape", "DEADBEAF00112233")]
        [TestCase("DEADBEAF00112233 11223344aabbccdd.ape", "11223344aabbccdd.ape", "DEADBEAF00112233")]
        public void ExtractSegments_When_FilenameLooksLikeHash_HasExtension_TreatAsFile(string input, string expectedFilename, string expectedHash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.AreEqual(expectedFilename, result.Key, "FileName");
            Assert.AreEqual(expectedHash, result.Value, "Hash");
        }

        [TestCase("DEADBEAF00112233 trackDEADBEAF00.flac#1", "trackDEADBEAF00.flac#1", "DEADBEAF00112233")]
        [TestCase(@"DEADBEAF00112233 Nirvana #1 - ""Lithium"".wav", @"Nirvana #1 - ""Lithium"".wav", "DEADBEAF00112233")]
        [TestCase(@"# DEADBEAF00112233 -> Nirvana #1 - ""Lithium"".wav", @"Nirvana #1 - ""Lithium"".wav", "DEADBEAF00112233")]
        public void ExtractSegments_HashFirst_Must_TreatAllTrailingTextAsFilename(string input, string expectedFilename, string expectedHash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.AreEqual(expectedFilename, result.Key, "FileName");
            Assert.AreEqual(expectedHash, result.Value, "Hash");
        }

        [TestCase("Smells   Like\tTeen  Spirit.flac DEADBEAF00112233", "Smells   Like\tTeen  Spirit.flac", "DEADBEAF00112233")]
        [TestCase("DEADBEAF00112233 Smells   Like Teen  Spirit\t.flac", "Smells   Like Teen  Spirit\t.flac", "DEADBEAF00112233")]
        [TestCase("Nirvana   :  in\tbloom.flac  -  DEADBEAF00112233", "Nirvana   :  in\tbloom.flac", "DEADBEAF00112233")]
        [TestCase("# [1]. Smells   Like\tTeen  Spirit.flac DEADBEAF00112233", "[1]. Smells   Like\tTeen  Spirit.flac", "DEADBEAF00112233")]
        public void ExtractSegments_Must_Preserve_InternalWhitespace_InFilename(string input, string expectedFilename, string expectedHash)
        {
            var result = RequireResult(target.Parse(input));

            Assert.AreEqual(expectedFilename, result.Key, "FileName");
            Assert.AreEqual(expectedHash, result.Value, "Hash");
        }

        [TestCase("DEADBEAF00112233 AABBCCDDEEFF0011")]
        [TestCase("slts.flac DEADBEAF00112233 AABBCCDDEEFF0011")]
        [TestCase("DEADBEAF00112233 slts.flac AABBCCDDEEFF0011")]
        [TestCase("DEADBEAF00112233 AABBCCDDEEFF0011 slts.flac")]
        [TestCase("DEADBEAF00112233 -> AABBCCDDEEFF0011")]
        public void When_MultipleHashes_Rejects(string input)
        {
            Assert.Throws<InvalidHashLineFormatException>(() => target.Parse(input));
        }

        [TestCase("slts.flac--DEADBEAF00112233")]
        [TestCase("DEADBEAF00112233--slts.flac")]
        [TestCase("slts.flac --DEADBEAF00112233")]
        [TestCase("DEADBEAF00112233** slts.flac")]
        public void When_InvalidSeparatorStructure_Rejects(string input)
        {
            Assert.Throws<InvalidHashLineFormatException>(() => target.Parse(input));
        }
    }
}
