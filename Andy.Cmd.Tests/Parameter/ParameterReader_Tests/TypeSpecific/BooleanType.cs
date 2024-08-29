using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.ParameterReader_Tests
{
    public class BooleanType
    {
        [TestCase("true", true)]
        [TestCase("false", false)]
        public void Nullable__Value_Provided(string rawValue, bool expected)
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

        [Test]
        public void Nullable__ParameterProvided_Value_NotProvided__Treat_As_True()
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", null }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Nullable));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.IsTrue(result.Nullable);
        }

        [TestCase("")]
        public void Nullable__Value_IsEmpty__Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [Test]
        public void Nullable__Parameter_NotSpecified__Return_Null()
        {
            var argvs = new Dictionary<string, string>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Nullable));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.IsNull(result.Nullable);
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public void Regular__Value_Provided(string rawValue, bool expected)
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

        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("True", true)]
        [TestCase("False", false)]
        [TestCase("TRUE", true)]
        [TestCase("FALSE", false)]
        [TestCase("1", true)]
        [TestCase("0", false)]
        public void Must_Support_All_Variations_Of_Boolean_Values(string rawValue, bool expected)
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

        [Test]
        public void Regular__ParameterProvided_Value_NotProvided__Treat_As_True()
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", null }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(true, result.Regular);
        }

        [TestCase("")]
        public void Regular__Value_IsEmpty__Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [Test]
        public void Regular__Parameter_NotSpecified__ErrorOut()
        {
            var argvs = new Dictionary<string, string>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterMissingException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public void RegularOptional__Value_Provided(string rawValue, bool expected)
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

        public void RegularOptional__ParameterProvided_Value_NotProvided__Treat_As_True()
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", null }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptional));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.IsTrue(result.RegularOptional);
        }

        [TestCase("")]
        public void RegularOptional__Value_IsEmpty__Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));

            Assert.Throws<ParameterEmptyException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [Test]
        public void RegularOptional__Parameter_NotSpecified__Return_False()
        {
            var argvs = new Dictionary<string, string>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptional));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.IsFalse(result.RegularOptional);
        }

        [Test]
        public void RegularOptional_WithDefaultValue__Parameter_NotSpecified__Return_DefaultValue()
        {
            var argvs = new Dictionary<string, string>();

            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptionalWithDefaultValue_True));
            ParameterReader.ReadParameter(prop, argvs, result);
            Assert.IsTrue(result.RegularOptionalWithDefaultValue_True, "Default value is True");

            var result2 = new TestParams();
            var prop2 = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptionalWithDefaultValue_False));
            ParameterReader.ReadParameter(prop2, argvs, result2);
            Assert.IsFalse(result.RegularOptionalWithDefaultValue_False, "Default value is False");
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
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalAllowEmpty));

            Assert.Throws<NotSupportedException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [TestCase("-")]
        [TestCase("-1")]
        [TestCase("2")]
        [TestCase("0.1")]
        [TestCase("10")]
        [TestCase("12.6")]
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
        [TestCase("-1")]
        [TestCase("2")]
        [TestCase("0.1")]
        [TestCase("10")]
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
        [TestCase("-1")]
        [TestCase("2")]
        [TestCase("0.1")]
        [TestCase("10")]
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
        [TestCase("-1")]
        [TestCase("2")]
        [TestCase("0.1")]
        [TestCase("10")]
        [TestCase("12.6")]
        public void Optional_WithDefaultValue__ValueOfWrongType__Must_Reject(string rawValue)
        {
            var argvs = new Dictionary<string, string>()
            {
                { "arg", rawValue }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.RegularOptionalWithDefaultValue_True));

            Assert.Throws<BadParameterValueException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }

        public static IEnumerable<TestCaseData> GetFor_AllowEmptyAttributeNotSupported()
        {
            return PrimitiveTypes.GetFor_AllowEmptyAttributeNotSupported();
        }

        class TestParams
        {
            [Parameter("arg")]
            public bool Regular { get; set; }

            [Parameter("arg")]
            [Optional]
            public bool RegularOptional { get; set; }

            [Parameter("arg")]
            [Optional(DefaultValue = true)]
            public bool RegularOptionalWithDefaultValue_True { get; set; }

            [Parameter("arg")]
            [Optional(DefaultValue = false)]
            public bool RegularOptionalWithDefaultValue_False { get; set; }

            [Parameter("arg")]
            public bool? Nullable { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public bool RegularAllowEmpty { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public bool? OptionalAllowEmpty { get; set; }
        }
    }
}