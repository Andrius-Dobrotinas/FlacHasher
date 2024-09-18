using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.ParameterReader_Tests
{
    public class General
    {
        [TestCase("--arg1", Description = "Problematic argument is provided")]
        [TestCase("unrelated", Description = "Problematic argument is not provided")]
        public void When_ParamterNameIsUsedMoreThanOnce__Must_Fail(string key)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { key, new [] { "arg 1 value" } }
            };

            Assert.Throws<InvalidOperationException>(() => ParameterReader.GetParameters<TestParamsNameClash>(argvs));
        }

        [Test]
        public void PopulateParameters_BasedOn_Name_From_ParameterAttribute()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg1", new [] { "arg 1 value" } },
                { "arg1", new [] { "Other Value" } },
            };
            var result = ParameterReader.GetParameters<TestParams>(argvs);

            Assert.AreEqual("arg 1 value", result.One, nameof(result.One));
            Assert.AreEqual("Other Value", result.Two, nameof(result.Two));
        }

        [Test]
        public void ParameterLookup_CaseInsensitive()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "arg3", new [] { "arg value" } }
            };
            var result = ParameterReader.GetParameters<TestParams2>(argvs, inLowercase: true);

            Assert.AreEqual("arg value", result.Three);
        }

        [Test]
        public void ParameterLookup_CaseSensitive()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "arg3", new [] { "arg value" } }
            };
            Assert.Throws<ParameterMissingException>(
                () => ParameterReader.GetParameters<TestParams2>(argvs, inLowercase: false));
        }

        class TestParams
        {
            [Parameter("--arg1")]
            public string One { get; set; }

            [Parameter("arg1")]
            public string Two { get; set; }
        }

        class TestParams2
        {
            [Parameter("ArG3")]
            public string Three { get; set; }
        }

        class TestParamsNameClash
        {
            [Parameter("--arg1")]
            public string One { get; set; }

            [Parameter("--arg1")]
            public string Two { get; set; }

            [Parameter("unrelated")]
            public string Unrelated { get; set; }
        }
    }
}