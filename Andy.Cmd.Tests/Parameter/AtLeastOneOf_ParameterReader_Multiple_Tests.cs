using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class AtLeastOneOf_ParameterReader_Multiple_Tests
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

        [TestCase("a value with spaces", "another one", "s'more")]
        [TestCase(null, "another one", "s'more")]
        [TestCase("a value with spaces", null, "s'more")]
        [TestCase("a value with spaces", "another one", null)]
        public void One_Parameter_FromEachGroup_HasValue__Other_DoesNot__Must_BeHappy(string shared, string value1, string value2)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Shared));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.One));
            var property3 = typeof(TestParams).GetProperty(nameof(TestParams.Two));
            Util.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, shared);
            Util.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, value1);
            Util.Set_ParameterValueResolver_Up<TestParams>(resolver, property3, value2);

            Assert.DoesNotThrow(() =>
                target.GetParameters<TestParams>(fakeArgs.Object));
        }
        
        [TestCase("one value", "another value", "third one")]
        [TestCase("a", "value, too", "xyz")]
        [TestCase("", "", "")]
        public void All_Paramters_HaveValues__Must_BeOverTheMoon(string shared, string value1, string value2)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Shared));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.One));
            var property3 = typeof(TestParams).GetProperty(nameof(TestParams.Two));
            Util.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, shared);
            Util.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, value1);
            Util.Set_ParameterValueResolver_Up<TestParams>(resolver, property3, value2);

            Assert.DoesNotThrow(() =>
                target.GetParameters<TestParams>(fakeArgs.Object));
        }

        [Test]
        public void Neither_Parameter_HasValue__Must_Reject()
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Shared));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.One));
            var property3 = typeof(TestParams).GetProperty(nameof(TestParams.Two));
            Util.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, null);
            Util.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, null);
            Util.Set_ParameterValueResolver_Up<TestParams>(resolver, property3, null);

            Assert.Throws<ParameterGroupException>(
                () => target.GetParameters<TestParams>(fakeArgs.Object));
        }

        class TestParams
        {
            [Parameter("arg1")]
            [AtLeastOneOf("key1")]
            [AtLeastOneOf("key2")]
            public string Shared { get; set; }

            [Parameter("arg2")]
            [AtLeastOneOf("key1")]
            public string One { get; set; }

            [Parameter("arg3")]
            [AtLeastOneOf("key2")]
            public string Two { get; set; }
        }
    }
}