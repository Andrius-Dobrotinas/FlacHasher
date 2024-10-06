using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class RequiredWith_ParameterResolution_Tests
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
        [TestCase("a value with spaces")]
        public void Used_With_OptionalAttr_WithDefaultValue__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional_DefaultValue));
            Assert.Throws<InvalidOperationException>(() => target.ReadParameter(prop, argvs, result));
        }

        class TestParams
        {
            [Parameter("param")]
            [RequiredWith("master")]
            public string Target { get; set; }

            [Parameter("param")]
            [RequiredWith("master")]
            [AllowEmpty]
            public string AllowEmpty { get; set; }

            [Parameter("param")]
            [RequiredWith("master")]
            [Optional(defaultValue: "something")]
            public string Optional_DefaultValue { get; set; }
        }
    }
}