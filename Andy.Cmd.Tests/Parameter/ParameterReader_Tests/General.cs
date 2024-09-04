using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.ParameterReader_Tests
{
    public class General
    {
        [Test]
        public void When_ParamterNameUsedMoreThanOnce__Must_Fail()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "arg1", new [] { "arg 1 value" } },
                { "-somethingElse", new [] {  "other value" } },
                { "caseinsensitive", new [] { "case insenstive value" } },
                { "CaseInsensitive", new [] { "Case Insenstive Value" }},
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
                { "caseinsensitive", new [] { "case insenstive value" }},
                { "CaseInsensitive", new [] { "Case Insenstive Value" }},
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

            [Parameter("caseinsensitive")]
            public string Three { get; set; }

            [Parameter("CaseInsensitive")]
            public string Four { get; set; }
        }

        class TestParamsNameClash
        {
            [Parameter("--arg1")]
            public string One { get; set; }

            [Parameter("--arg1")]
            public string Two { get; set; }
        }
    }
}