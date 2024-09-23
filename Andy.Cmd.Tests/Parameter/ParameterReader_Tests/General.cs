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
        public void NotUse_Inherited_ParameterConfiguration__When_OverrideHasNoParamAttr()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg1-base", new [] { "arg 1 value" } },
            };
            var result = ParameterReader.GetParameters<TestParamsInheritance1>(argvs);

            Assert.IsNull(result.One);
        }

        [Test]
        public void NotUse_Inherited_ParameterConfiguration__When_OverrideHasParamAttr()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg1-base", new [] { "arg 1 base" } },
                { "--arg1-new", new [] { "arg 1 new" } },
            };
            var result = ParameterReader.GetParameters<TestParamsInheritance2>(argvs);

            Assert.AreEqual("arg 1 new", result.One, nameof(result.One));
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

        [TestCase("first value")]
        [TestCase("second value")]
        public void When_MultipleParamtersSpecified_AllArePresent__Must_TakeOneWithLowerOrder(string firstValue)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg1", new [] { firstValue } },
                { "Arg1", new [] { "second value" } },
                { "Three", new [] { "third value" } }
            };

            var result = new TestParamsMultiple();
            var prop = typeof(TestParamsMultiple).GetProperties().First(x => x.Name == nameof(TestParamsMultiple.One));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(firstValue, result.One);
        }

        [TestCase("first value")]
        [TestCase("second value")]
        public void When_MultipleParametersSpecified_OnlyOneIsNotPresent__Must_TakeTheAvailableValue(string firstValue)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--somethinElse", new [] { "value" } },
                { "--arg1", new [] { firstValue } }
            };

            var result = new TestParamsMultiple();
            var prop = typeof(TestParamsMultiple).GetProperties().First(x => x.Name == nameof(TestParamsMultiple.One));
            ParameterReader.ReadParameter(prop, argvs, result);

            Assert.AreEqual(firstValue, result.One);
        }

        [Test]
        public void When_MultipleParametersSpecified_NoneArePresent__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--somethinElse", new [] { "value" } }
            };

            var result = new TestParamsMultiple();
            var prop = typeof(TestParamsMultiple).GetProperties().First(x => x.Name == nameof(TestParamsMultiple.One));
            Assert.Throws<ParameterMissingException>(
                () => ParameterReader.ReadParameter(prop, argvs, result));
        }

        [Test]
        public void When_Optional_MultipleParametersSpecified_NoneArePresent__Must_BeCool()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--somethinElse", new [] { "value" } }
            };

            var result = new TestParamsMultiple();
            var prop = typeof(TestParamsMultiple).GetProperties().First(x => x.Name == nameof(TestParamsMultiple.Two));
            ParameterReader.ReadParameter(prop, argvs, result);
            Assert.IsNull(result.Two);
        }

        [TestCase("--arg1", Description = "Problematic argument is provided")]
        [TestCase("unrelated", Description = "Problematic argument is not provided")]
        public void When_MultipleParametersSpecified__ParamterNameIsUsedMoreThanOnce__Must_Fail(string key)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { key, new [] { "arg 1 value" } }
            };
            Assert.Throws<InvalidOperationException>(() => ParameterReader.GetParameters<TestParamsNameClashMulti>(argvs));
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

        class TestParamsNameClashMulti
        {
            [Parameter("--arg1")]
            [Parameter("--arg1")]
            public string One { get; set; }
        }

        class TestParamsMultiple
        {
            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public string One { get; set; }

            [Parameter("--arg2")]
            [Parameter("ArgToo")]
            [Optional]
            public string Two { get; set; }
        }

        class TestParamsBase
        {
            [Parameter("--arg1-base")]
            public virtual string One { get; set; }
        }

        class TestParamsInheritance1 : TestParamsBase
        {
            public override string One { get; set; }
        }

        class TestParamsInheritance2 : TestParamsBase
        {
            [Parameter("--arg1-new")]
            public override string One { get; set; }
        }
    }
}