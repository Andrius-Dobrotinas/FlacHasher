using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.ParameterReader_Tests
{
    public class TypeString
    {
        [Test]
        public void String__Parameter_NotProvided__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterMissingException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("0")]
        [TestCase("false")]
        [TestCase(".")]
        [TestCase("a value with spaces")]
        public void String__Value_Provided__Must_UseIt(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(value, result.Regular);
        }

        [TestCase(new[] { "1", "10" }, "10")]
        [TestCase(new[] { "2", "1", "0" }, "0")]
        [TestCase(new[] { "-1" }, "-1")]
        public void When_Paramter_IsRepeated__Must_Take_Last_Value(string[] input, string expected)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", input }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, result.Regular);
        }

        [TestCase(" 0", "0")]
        [TestCase("false ", "false")]
        [TestCase(" a value with spaces ", "a value with spaces")]
        public void Must_Trim_The_Value(string value, string expected)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, result.Regular);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void String__Value_NotProvided__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void String_AllowEmpty__Value_EmptyStringOrWhitespaceProvided__Must_ReturnEmptyString(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new string[] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularEmptyAllowed));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual("", result.RegularEmptyAllowed);
        }

        [Test]
        public void String_AllowEmpty__Value_NotProvided__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new string[] { null } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularEmptyAllowed));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [Test]
        public void String_Optional__Parameter_NotProvided__Must_Return_Null()
        {
            var argvs = new Dictionary<string, string[]>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.IsNull(result.Optional);
        }

        [TestCase("0")]
        [TestCase("false")]
        [TestCase(".")]
        [TestCase("a value with spaces")]
        public void String_Optional__Value_Provided__Must_UseIt(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(value, result.Optional);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void String_Optional__Value_NotProvided__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void String_Optional_AllowEmpty__EmptyStringOrWhitespaceProvided__Must_ReturnEmptyString(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalEmptyAllowed));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual("", result.OptionalEmptyAllowed);
        }

        [Test]
        public void String_Optional_AllowEmpty__Value_NotProvided__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new string[] { null } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalEmptyAllowed));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("10")]
        [TestCase("0")]
        [TestCase("-1")]
        public void Optional_WithDefaultValue__Value_Provided__Must_UseIt(string rawValue)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { rawValue } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalDefaultValue));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(rawValue, result.OptionalDefaultValue);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void Optional_WithDefaltValue__Parameter_Provided_NoValue__Must_Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { rawValue } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalDefaultValue));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [Test]
        public void Optional_WithDefaultValue__Parameter_NotProvided__Return_DefaultValue()
        {
            var argvs = new Dictionary<string, string[]>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalDefaultValue));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual("x", result.OptionalDefaultValue);
        }

        class TestParams
        {
            [Parameter("arg")]
            public string Regular { get; set; }

            [Parameter("arg")]
            [AllowEmpty()]
            public string RegularEmptyAllowed { get; set; }

            [Parameter("arg")]
            [Optional]
            public string Optional { get; set; }

            [Parameter("arg")]
            [Optional]
            [AllowEmpty()]
            public string OptionalEmptyAllowed { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: "x")]
            public string OptionalDefaultValue { get; set; }
        }
    }
}