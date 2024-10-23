using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class AtLeastOneOf_ParameterResolution_Tests
    {
        ParameterValueResolver target = new ParameterValueResolver();

        [Test]
        public void TreatAs_Optional__When_Param_NotProvided()
        {
            var argvs = new Dictionary<string, string[]>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Target));
            target.ReadParameter<TestParams>(prop, argvs, result);

            Assert.IsNull(result.Target);
        }

        [Test]
        public void AllowEmpty_When_Decorated_With_AllowEmptyAttribute()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "param", new [] { "" } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.AllowEmpty));
            target.ReadParameter<TestParams>(prop, argvs, result);

            Assert.AreEqual("", result.AllowEmpty);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Used_With_OptionalAttr_WithDefaultValue__And_HasNoValue__Must_Use_DefaultValue(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional_DefaultValue));
            target.ReadParameter<TestParams>(prop, argvs, result);

            Assert.AreEqual("something", result.Optional_DefaultValue);
        }

        [TestCase("a value with spaces")]
        public void Used_With_OptionalAttr_WithDefaultValue__And_HasValue__Must_Ignore_DefaultValue(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional_DefaultValue));
            target.ReadParameter<TestParams>(prop, argvs, result);

            Assert.AreEqual("something", result.Optional_DefaultValue);
        }

        class TestParams
        {
            [Parameter("param")]
            [AtLeastOneOf("key")]
            public string Target { get; set; }

            [Parameter("param")]
            [AtLeastOneOf("key")]
            [AllowEmpty]
            public string AllowEmpty { get; set; }

            [Parameter("param")]
            [AtLeastOneOf("key")]
            [Optional(defaultValue: "something")]
            public string Optional_DefaultValue { get; set; }
        }
    }
}