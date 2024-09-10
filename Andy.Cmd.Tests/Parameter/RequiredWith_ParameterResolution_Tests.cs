using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class RequiredWith_ParameterResolution_Tests
    {
        [Test]
        public void When_Param_NotProvided__Treat_AsOptional()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "master", new [] { "good value" } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Dependency));
            ParameterReader.ReadParameter<TestParams>(prop, argvs, result);

            Assert.IsNull(result.Dependency);
        }

        [Test]
        public void When_Empty__With_AllowEmptyAttribute__Treat_AsOptional()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "dependency", new [] { "" } },
                { "master", new [] { "good value" } }
            };
            var result = new TestParamsAllowEmpty();
            var prop = typeof(TestParamsAllowEmpty).GetProperties().First(x => x.Name == nameof(TestParamsAllowEmpty.Dependency));
            ParameterReader.ReadParameter<TestParamsAllowEmpty>(prop, argvs, result);

            Assert.AreEqual("", result.Dependency);
        }

        class TestParams
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency")]
            [RequiredWith(nameof(Master))]
            public string Dependency { get; set; }
        }

        class TestParamsAllowEmpty
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency")]
            [RequiredWith(nameof(Master))]
            [AllowEmpty]
            public string Dependency { get; set; }
        }
    }
}