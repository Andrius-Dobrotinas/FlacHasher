using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.ParameterValueResolver_Tests.TypeSpecific
{
    public class BooleanType
    {
        ParameterValueResolver target = new ParameterValueResolver();

        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("True", true)]
        [TestCase("False", false)]
        [TestCase("TRUE", true)]
        [TestCase("FALSE", false)]
        [TestCase("1", true)]
        [TestCase("0", false)]
        public void Must_Support_AllCommon_StringRepresentations_Of_Boolean_Values(string rawValue, bool expected)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { rawValue } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.Regular));
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, result.Regular);
        }

        [TestCase(nameof(TestParams.Regular))]
        [TestCase(nameof(TestParams.RegularOptional))]
        [TestCase(nameof(TestParams.Nullable))]
        [TestCase(nameof(TestParams.RegularOptionalWithDefaultValue_False))]
        [TestCase(nameof(TestParams.RegularOptionalWithDefaultValue_True))]
        public void ParameterProvided_But_Value_NotProvided__Must__Treat_As_True__RegardlessOfWhetherItIsOptional_AndItsDefaultValue(string propertyName)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new string[] { null } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(true, prop.GetValue(result));
        }

        class TestParams
        {
            [Parameter("arg")]
            public bool Regular { get; set; }

            [Parameter("arg")]
            [Optional]
            public bool RegularOptional { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: true)]
            public bool RegularOptionalWithDefaultValue_True { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: false)]
            public bool RegularOptionalWithDefaultValue_False { get; set; }

            [Parameter("arg")]
            public bool? Nullable { get; set; }
        }
    }
}