using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.EitherOr_Tests.ParameterResolution_Tests
{
    public class EitherOrTests_Base<TTestParams>
        where TTestParams : TestParams, new()
    {
        protected ParameterValueResolver target = new ParameterValueResolver();

        [Test]
        public void TreatAs_Optional__When_Param_NotProvided()
        {
            var argvs = new Dictionary<string, string[]>();
            var result = new TTestParams();
            var prop = typeof(TTestParams).GetProperties().First(x => x.Name == nameof(TestParams.Target));
            target.ReadParameter(prop, argvs, result);

            Assert.IsNull(result.Target);
        }

        [Test]
        public void AllowEmpty_When_Decorated_With_AllowEmptyAttribute()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "param", new [] { "" } }
            };
            var result = new TTestParams();
            var prop = typeof(TTestParams).GetProperties().First(x => x.Name == nameof(TestParams.AllowEmpty));
            target.ReadParameter(prop, argvs, result);

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
            var result = new TTestParams();
            var prop = typeof(TTestParams).GetProperties().First(x => x.Name == nameof(TestParams.Optional_DefaultValue));
            Assert.Throws<InvalidOperationException>(() => target.ReadParameter(prop, argvs, result));
        }
    }

    public abstract class TestParams
    {
        [Parameter("param")]
        [EitherOr("group")]
        public abstract string Target { get; set; }

        [Parameter("param")]
        [EitherOr("group")]
        [AllowEmpty]
        public abstract string AllowEmpty { get; set; }

        [Parameter("param")]
        [RequiredWith("master")]
        [Optional(defaultValue: "something")]
        public abstract string Optional_DefaultValue { get; set; }
    }
}