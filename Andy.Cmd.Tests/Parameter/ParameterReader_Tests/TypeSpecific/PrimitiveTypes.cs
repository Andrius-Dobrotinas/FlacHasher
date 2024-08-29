using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.ParameterReader_Tests
{
    public class PrimitiveTypes
    {
        [Test]
        public void Regular__Parameter_NotProvided__Must_Reject()
        {
            var argvs = new Dictionary<string, string>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterMissingException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("10", 10)]
        [TestCase("0", 0)]
        [TestCase("-1", -1)]
        public void Regular__Value_Provided__Must_ParseIt(string rawValue, int expected)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, result.Regular);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Regular__Parameter_Provided_NoValue__Must_Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("10", 10)]
        [TestCase("0", 0)]
        [TestCase("-1", -1)]
        public void Optional__Value_Provided__Must_UseIt(string rawValue, int expected)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptional));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, result.RegularOptional);
        }

        [Test]
        public void Optional__Parameter_NotProvided__Return_DefaultValueForTheType()
        {
            var argvs = new Dictionary<string, string>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptional));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(default(int), result.RegularOptional);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Optional__Parameter_Provided_NoValue__Must_Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptional));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("10", 10)]
        [TestCase("0", 0)]
        [TestCase("-1", -1)]
        public void Optional_WithDefaultValue__Value_Provided__Must_UseIt(string rawValue, int expected)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptionalWithDefault));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, result.RegularOptionalWithDefault);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Optional_WithDefaltValue__Parameter_Provided_NoValue__Must_Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptionalWithDefault));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [Test]
        public void Optional_WithDefaultValue__Parameter_NotProvided__Return_DefaultValue()
        {
            var argvs = new Dictionary<string, string>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptionalWithDefault));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(6, result.RegularOptionalWithDefault);
        }

        [TestCase("10", 10)]
        [TestCase("0", 0)]
        [TestCase("-1", -1)]
        public void Nullable__Value_Provided__Must_UseIt(string rawValue, int expected)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Nullable));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, result.Nullable);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Nullable__Parameter_Provided_NoValue__Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Nullable));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [Test]
        public void Nullable__Parameter_NotProvided__Return_Null()
        {
            var argvs = new Dictionary<string, string>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Nullable));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.IsNull(result.Nullable);
        }

        [TestCaseSource(nameof(GetFor_AllowEmptyAttributeNotSupported))]
        public void AllowEmptyAttribute__NotSupported(Dictionary<string, string> argvs)
        {
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularAllowEmpty));

            Assert.Throws<NotSupportedException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCaseSource(nameof(GetFor_AllowEmptyAttributeNotSupported))]
        public void Nullable_AllowEmptyAttribute__NotSupported(Dictionary<string, string> argvs)
        {
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.NullableAllowEmpty));

            Assert.Throws<NotSupportedException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("-")]
        [TestCase("false")]
        [TestCase("12.6")]
        [TestCase("12,6")]
        [TestCase("12/6")]
        public void Regular__ValueOfWrongType__Must_Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>()
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<BadParameterValueException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("-")]
        [TestCase("false")]
        [TestCase("12.6")]
        public void Nullable__ValueOfWrongType__Must_Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>()
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Nullable));

            Assert.Throws<BadParameterValueException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("-")]
        [TestCase("false")]
        [TestCase("12.6")]
        public void Optional__ValueOfWrongType__Must_Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>()
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptional));

            Assert.Throws<BadParameterValueException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("-")]
        [TestCase("false")]
        [TestCase("12.6")]
        public void Optional_WithDefaultValue__ValueOfWrongType__Must_Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>()
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptionalWithDefault));

            Assert.Throws<BadParameterValueException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        public static IEnumerable<TestCaseData> GetFor_AllowEmptyAttributeNotSupported()
        {
            yield return new TestCaseData(
                new Dictionary<string, string>
                {
                    { "arg", "1" }
                });

            yield return new TestCaseData(
                new Dictionary<string, string>
                {
                    { "arg", "" }
                });

            yield return new TestCaseData(
                new Dictionary<string, string>
                {
                    { "arg", " " }
                });

            yield return new TestCaseData(
                new Dictionary<string, string>
                {
                    { "arg", null }
                });

            yield return new TestCaseData(
                new Dictionary<string, string>());
        }

        class TestParams
        {
            [Parameter("arg")]
            public int Regular { get; set; }

            [Parameter("arg")]
            [Optional]
            public int RegularOptional { get; set; }

            [Parameter("arg")]
            [Optional(DefaultValue = 6)]
            public int RegularOptionalWithDefault { get; set; }

            [Parameter("arg")]
            public int? Nullable { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public int RegularAllowEmpty { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public int? NullableAllowEmpty { get; set; }
        }
    }
}