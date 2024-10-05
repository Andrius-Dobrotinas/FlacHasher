using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.ParameterReader_Tests
{
    public class General_Reader
    {
        ParameterReader target;
        Mock<ParameterValueResolver> resolver;

        public General_Reader()
        {
            resolver = new Mock<ParameterValueResolver>();
            target = new ParameterReader(resolver.Object);
        }

        [TestCase("--arg1", Description = "Problematic argument is provided")]
        [TestCase("unrelated", Description = "Problematic argument is not provided")]
        public void When_ParamterName_Used_MoreThanOnce__Must_Fail(string key)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { key, new [] { "arg 1 value" } }
            };

            Assert.Throws<InvalidOperationException>(() => target.GetParameters<TestParamsNameClash>(argvs));
        }

        [TestCase("--arg1", Description = "Problematic argument is provided")]
        [TestCase("unrelated", Description = "Problematic argument is not provided")]
        public void When_Multiple_ParametersSpecified__Parameter_Name_Used_MoreThanOnce__Must_Fail(string key)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { key, new [] { "arg 1 value" } }
            };
            Assert.Throws<InvalidOperationException>(() => target.GetParameters<TestParamsNameClashMulti>(argvs));
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

        class TestParamsNameClashMulti
        {
            [Parameter("--arg1")]
            [Parameter("--arg1")]
            public string One { get; set; }
        }
    }
}