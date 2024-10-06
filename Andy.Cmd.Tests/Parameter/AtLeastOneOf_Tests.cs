using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class AtLeastOneOf_Tests
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

        [TestCase( "0", null)]
        [TestCase(null, "a value with spaces")]
        [TestCase("", null)]
        [TestCase(null, "")]
        public void One_Parameter_HasValue__Other_Does_Not__Must_BeHappy(string value1, string value2)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.One));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.Two));
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, value1);
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, value2);

            Assert.DoesNotThrow(() =>
                target.GetParameters<TestParams>(fakeArgs.Object));
        }

        [TestCase("one value", "another value")]
        [TestCase("a", "value, too")]
        public void Both_Paramters_HaveValues__Must_BeOverTheMoon(string value1, string value2)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.One));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.Two));
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, value1);
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, value2);

            Assert.DoesNotThrow(() =>
                target.GetParameters<TestParams>(fakeArgs.Object));
        }

        [Test]
        public void Neither_Parameter_HasValue__Must_Reject()
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.One));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.Two));
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, null);
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, null);

            Assert.Throws<ParameterGroupException>(
                () => target.GetParameters<TestParams>(fakeArgs.Object));
        }

        class TestParams
        {
            [Parameter("arg1")]
            [AtLeastOneOf("key1")]
            public string One { get; set; }

            [Parameter("arg2")]
            [AtLeastOneOf("key1")]
            public string Two { get; set; }
        }
    }
}