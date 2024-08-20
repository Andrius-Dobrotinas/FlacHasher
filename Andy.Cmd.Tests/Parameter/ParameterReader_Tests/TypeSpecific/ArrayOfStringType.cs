using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.ParameterReader_Tests
{
    public class ArrayOfStringType
    {
        [Test]
        public void Regular__Parameter_NotProvided__Must_Reject()
        {
            var argvs = new Dictionary<string, string>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterMissingException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCase("0;One;three", new[] { "0", "One", "three" })]
        [TestCase("2", new[] { "2" })]
        [TestCase("First;Se Cond", new[] { "First", "Se Cond" })]
        public void Regular__Value_Provided__Must_ParseItAsSemiColonSeparated(string value, string[] expectedValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));
            ParameterReader.Parse(prop, argvs, result);

            Assert.AreEqual(expectedValue, result.Regular);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Regular__Value_NotProvided__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCase(";")]
        [TestCase(";;;")]
        public void Regular__Value_Provided_SeparatorsOnly__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));
            Assert.Throws<BadParameterValueException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCase(";")]
        [TestCase(";;;")]
        public void Optional__Value_Provided_SeparatorsOnly__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional));
            Assert.Throws<BadParameterValueException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCase(";")]
        [TestCase(";;;")]
        public void Optional_AllowEmpty__Value_Provided_AllElementsEmpty__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalEmptyAllowed));
            Assert.Throws<BadParameterValueException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCase("one;")]
        [TestCase(";two")]
        [TestCase("two;;three")]
        public void Optional_AllowEmpty__Value_Provided_SomeElementsEmpty__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalEmptyAllowed));
            Assert.Throws<BadParameterValueException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [Test]
        public void Optional__Parameter_NotProvided__Must_Return_Null()
        {
            var argvs = new Dictionary<string, string>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional));
            ParameterReader.Parse(prop, argvs, result);

            Assert.IsNull(result.Optional);
        }

        [TestCase("0;One;three", new[] { "0", "One", "three" })]
        [TestCase("2", new[] { "2" })]
        [TestCase("First;Se Cond", new[] { "First", "Se Cond" })]
        public void Optional__Value_Provided__Must_ParseItAsSemiColonSeparated(string value, string[] expected)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional));
            ParameterReader.Parse(prop, argvs, result);

            Assert.AreEqual(expected, result.Optional);
        }

        [TestCase("First;Se Cond;")]
        [TestCase(";")]
        public void Optional__Value_Provided__ContainsEmptyElements__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional));

            Assert.Throws<BadParameterValueException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCase("First;Se Cond;")]
        [TestCase(";")]
        public void Regular__Value_Provided__ContainsEmptyElements__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<BadParameterValueException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Optional__ParameterProvided_Value_NotProvided__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCase("")]
        public void Regular_AllowEmpty__Value_EmptyStringProvided__NotSupported(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.EmptyAllowed));

            Assert.Throws<NotSupportedException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [Test]
        public void Regular_AllowEmpty__Value_NotProvided__NotSupported()
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", null }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.EmptyAllowed));

            Assert.Throws<NotSupportedException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCase("")]
        public void Optional_AllowEmpty__Value_EmptyStringProvided__Must_ReturnEmptyArray(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", value }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalEmptyAllowed));
            ParameterReader.Parse(prop, argvs, result);

            Assert.AreEqual(new string[0], result.OptionalEmptyAllowed);
        }

        [Test]
        public void Optional_AllowEmpty__Value_NotProvided__Must_Reject()
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", null }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalEmptyAllowed));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        [TestCaseSource(nameof(GetFor_DefaultValueNotSupported))]
        public void Optional_DefaulValue__NotSupported(Dictionary<string, string> argvs)
        {
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalDefaultValue));

            Assert.Throws<NotSupportedException>(() => ParameterReader.Parse(prop, argvs, result));
        }

        public static IEnumerable<TestCaseData> GetFor_DefaultValueNotSupported()
        {
            return PrimitiveTypes.GetFor_AllowEmptyAttributeNotSupported();
        }

        class TestParams
        {
            [Parameter("arg")]
            public string[] Regular { get; set; }

            [Parameter("arg")]
            [Optional]
            public string[] Optional { get; set; }

            [Parameter("arg")]
            [AllowEmpty()]
            public string[] EmptyAllowed { get; set; }

            [Parameter("arg")]
            [Optional]
            [AllowEmpty()]
            public string[] OptionalEmptyAllowed { get; set; }

            [Parameter("arg")]
            [Optional(DefaultValue = new[] { "one" })]
            public string[] OptionalDefaultValue { get; set; }
        }
    }
}