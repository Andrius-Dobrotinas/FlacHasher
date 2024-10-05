using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
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
            var argvs = new Dictionary<string, string[]>
            {
                { "master", new [] { "good value" } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Dependency));
            target.ReadParameter<TestParams>(prop, argvs, result);

            Assert.IsNull(result.Dependency);
        }

        [Test]
        public void TreatAs_Optional__When_Empty_With_AllowEmptyAttribute()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "dependency", new [] { "" } },
                { "master", new [] { "good value" } }
            };
            var result = new TestParamsAllowEmpty();
            var prop = typeof(TestParamsAllowEmpty).GetProperties().First(x => x.Name == nameof(TestParamsAllowEmpty.Dependency));
            target.ReadParameter<TestParamsAllowEmpty>(prop, argvs, result);

            Assert.AreEqual("", result.Dependency);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("a value with spaces")]
        public void UsedWith_Optional_HavingDefaultValue__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { value } }
            };
            var result = new TestParams2();
            var prop = typeof(TestParams2).GetProperties().First(x => x.Name == nameof(TestParams2.One));
            Assert.Throws<InvalidOperationException>(() => target.ReadParameter(prop, argvs, result));
        }

        class TestParams
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency")]
            [RequiredWithAttribute(nameof(Master))]
            public string Dependency { get; set; }
        }

        class TestParamsAllowEmpty
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency")]
            [RequiredWithAttribute(nameof(Master))]
            [AllowEmpty]
            public string Dependency { get; set; }
        }

        class TestParams2
        {
            [Parameter("arg1")]
            [EitherOr("key1")]
            [Optional(defaultValue: "something")]
            public string One { get; set; }
        }
    }
}