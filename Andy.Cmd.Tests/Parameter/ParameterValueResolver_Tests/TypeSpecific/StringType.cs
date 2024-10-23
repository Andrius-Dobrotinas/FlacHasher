using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.ParameterValueResolver_Tests.TypeSpecific
{
    public class StringType
    {
        ParameterValueResolver target = new ParameterValueResolver();

        [TestCase(nameof(TestParams.Regular), " 0", "0")]
        [TestCase(nameof(TestParams.Regular), "false ", "false")]
        [TestCase(nameof(TestParams.Regular), " a value with spaces ", "a value with spaces")]
        [TestCase(nameof(TestParams.Optional), " 0 ", "0")]
        [TestCase(nameof(TestParams.RegularEmptyAllowed), " 0 ", "0")]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), " 0 ", "0")]
        public void Trim_The_Value(string propertyName, string value, string expected)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, prop.GetValue(result));
        }

        class TestParams
        {
            [Parameter("arg")]
            public string Regular { get; set; }

            [Parameter("arg")]
            [AllowEmpty()]
            public string RegularEmptyAllowed { get; set; }

            [Parameter("arg")]
            [Optional]
            public string Optional { get; set; }

            [Parameter("arg")]
            [Optional]
            [AllowEmpty()]
            public string OptionalEmptyAllowed { get; set; }
        }
    }
}