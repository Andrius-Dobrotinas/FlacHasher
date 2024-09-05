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
                { key, new [] { "arg 1 value" } },
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
            Assert.AreEqual("case insenstive value", result.Three, nameof(result.Three));
            Assert.AreEqual("Case Insenstive Value", result.Four, nameof(result.Four));
        }

        class TestParams
        {
            [Parameter("--arg1")]
            public string One { get; set; }

            [Parameter("arg1")]
            public string Two { get; set; }
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