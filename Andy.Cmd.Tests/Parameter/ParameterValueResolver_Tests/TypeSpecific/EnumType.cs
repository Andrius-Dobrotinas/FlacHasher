using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter.ParameterValueResolver_Tests.TypeSpecific
{
    public class EnumType
    {
        ParameterValueResolver target = new ParameterValueResolver();

        [TestCase(nameof(TestParams.Regular), nameof(TestEnum.One), TestEnum.One)]
        [TestCase(nameof(TestParams.Regular), nameof(TestEnum.Two), TestEnum.Two)]
        [TestCase(nameof(TestParams.Regular), nameof(TestEnum.Three), TestEnum.Three)]
        [TestCase(nameof(TestParams.Regular), "0", TestEnum.One)]
        [TestCase(nameof(TestParams.Regular), "1", TestEnum.Two)]
        [TestCase(nameof(TestParams.Regular), "2", TestEnum.Three)]
        [TestCase(nameof(TestParams.Regular), "one", TestEnum.One)]
        [TestCase(nameof(TestParams.Regular), "two", TestEnum.Two)]
        [TestCase(nameof(TestParams.Regular), "three", TestEnum.Three)]
        [TestCase(nameof(TestParams.Interesting), nameof(TestEnumInteresting.Ichi), TestEnumInteresting.Ichi)]
        [TestCase(nameof(TestParams.Interesting), nameof(TestEnumInteresting.Ni), TestEnumInteresting.Ni)]
        [TestCase(nameof(TestParams.Interesting), nameof(TestEnumInteresting.Go), TestEnumInteresting.Go)]
        [TestCase(nameof(TestParams.Interesting), "-1", TestEnumInteresting.Ichi)]
        [TestCase(nameof(TestParams.Interesting), "1", TestEnumInteresting.Ni)]
        [TestCase(nameof(TestParams.Interesting), "3", TestEnumInteresting.Go)]
        [TestCase(nameof(TestParams.Interesting), "ichi", TestEnumInteresting.Ichi)]
        [TestCase(nameof(TestParams.Interesting), "ni", TestEnumInteresting.Ni)]
        [TestCase(nameof(TestParams.Interesting), "go", TestEnumInteresting.Go)]

        public void Must_Support_AllCommon_StringRepresentations_Of_Enum_Values(string propertyName, string rawValue, object expected)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { rawValue } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.Regular), "Four")]
        [TestCase(nameof(TestParams.Regular), "Zero")]
        [TestCase(nameof(TestParams.Regular), "Whatevz")]
        [TestCase(nameof(TestParams.Regular), "-1")]
        [TestCase(nameof(TestParams.Regular), "5")]
        [TestCase(nameof(TestParams.Regular), "1.1")]
        [TestCase(nameof(TestParams.Regular), "false")]
        [TestCase(nameof(TestParams.Regular), "true")]
        [TestCase(nameof(TestParams.Regular), "[]")]
        [TestCase(nameof(TestParams.Regular), "TestEnum.One")]
        [TestCase(nameof(TestParams.Interesting), "Four")]
        [TestCase(nameof(TestParams.Interesting), "Zero")]
        [TestCase(nameof(TestParams.Interesting), "Whatevz")]
        [TestCase(nameof(TestParams.Interesting), "-2")]
        [TestCase(nameof(TestParams.Interesting), "0")]
        [TestCase(nameof(TestParams.Interesting), "2")]
        [TestCase(nameof(TestParams.Interesting), "1.1")]
        [TestCase(nameof(TestParams.Interesting), "false")]
        [TestCase(nameof(TestParams.Interesting), "true")]
        [TestCase(nameof(TestParams.Interesting), "[]")]
        [TestCase(nameof(TestParams.Interesting), "TestEnumInteresting.Ichi")]
        public void When__ParameterValue_Is_OfWrongType__Must_Reject(string propertyName, string rawValue)
        {

            var argvs = new Dictionary<string, string[]>()
            {
                { "arg", new [] { rawValue } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);

            Assert.Throws<BadParameterValueException>(() => target.ReadParameter(prop, argvs, result));
        }

        class TestParams
        {
            [Parameter("arg")]
            public TestEnum Regular { get; set; }

            [Parameter("arg")]
            public TestEnumInteresting Interesting { get; set; }
        }

        public enum TestEnum
        {
            One = 0,
            Two = 1,
            Three = 2,
        }

        public enum TestEnumInteresting
        {
            Ichi = -1,
            Ni = 1,
            Go = 3,
        }
    }
}