using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.EitherOr_Tests.ParameterReader_Tests
{
    public class EitherOr_Combinations_Tests
    {
        ParameterReader target;
        Mock<IParameterValueResolver> resolver;
        Mock<IDictionary<string, string[]>> fakeArgs;

        [SetUp]
        public void SetUp()
        {
            resolver = new Mock<IParameterValueResolver>();
            target = new ParameterReader(resolver.Object);

            fakeArgs = new Mock<IDictionary<string, string[]>>();
        }

        [Test]
        public void Keep_SameNameGroups_From_DifferentAttributes_Separate__When_Two_Single_Parameters_MissingTheirCounterparts()
        {
            Assert.Throws<InvalidOperationException>(() => target.GetParameters<TestParams1>(fakeArgs.Object));
        }

        [Test]
        public void Keep_SameNameGroups_From_DifferentAttributes_Separate__When_Each_Group_Has_A_Value()
        {
            var property1 = typeof(TestParams2).GetProperty(nameof(TestParams2.One));
            var property3 = typeof(TestParams2).GetProperty(nameof(TestParams2.Three));
            Util.Set_ParameterValueResolver_Up<TestParams2>(resolver, property1, "asd1");
            Util.Set_ParameterValueResolver_Up<TestParams2>(resolver, property3, "asd3");

            Assert.DoesNotThrow(() => target.GetParameters<TestParams2>(fakeArgs.Object));
        }
    }

    public class TestParams1
    {
        [Parameter("param1")]
        [OptionalEitherOr("group")]
        public string One { get; set; }

        [Parameter("param2")]
        [EitherOr("group")]
        public string Two { get; set; }
    }

    public class TestParams2 : TestParams1
    {
        [Parameter("param4")]
        [OptionalEitherOr("group")]
        public string Four { get; set; }

        [Parameter("param3")]
        [EitherOr("group")]
        public string Three { get; set; }
    }
}