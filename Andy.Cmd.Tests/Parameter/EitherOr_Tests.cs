using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public class EitherOr_Tests
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

        [TestCase(nameof(TestParams.One), "Wassup?")]
        [TestCase(nameof(TestParams.One), "a value with spaces")]
        [TestCase(nameof(TestParams.One), "")]
        [TestCase(nameof(TestParams.Two), 1)]
        [TestCase(nameof(TestParams.Two), -1)]
        [TestCase(nameof(TestParams.Two), 0)]
        [TestCase(nameof(TestParams.Three), new[] { "one" })]
        [TestCase(nameof(TestParams.Three), new[] { "" })]
        [TestCase(nameof(TestParams.Three), new string[0])]
        public void One_Parameter_HasValue__Other_Not__Must_Pass(string propertyName, object value)
        {
            var property = typeof(TestParams).GetProperty(propertyName);
            Set_ParameterValueResolver_Up<TestParams>(property, value);

            Assert.DoesNotThrow(() => target.GetParameters<TestParams>(fakeArgs.Object));
        }

        
        [TestCase("ichi", 1, new[] { "a"})]
        [TestCase("ni", null, new[] { "b"})]
        [TestCase("san", 3, null)]
        [TestCase(null, 667, new[] { "c"})]
        public void More_Than_One_Paramter_HasValues__Must_Reject(string firstArgValue, int? secondArgValue, string[] thirdArgValue)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.One));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.Two));
            var property3 = typeof(TestParams).GetProperty(nameof(TestParams.Three));
            Set_ParameterValueResolver_Up<TestParams>(property1, firstArgValue);
            Set_ParameterValueResolver_Up<TestParams>(property2, secondArgValue);
            Set_ParameterValueResolver_Up<TestParams>(property3, thirdArgValue);

            Assert.Throws<ParameterGroupException>(
                () => target.GetParameters<TestParams>(fakeArgs.Object));
        }

        [Test]
        public void Neither_Parameter_HasValue__Must_Reject()
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.One));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.Two));
            var property3 = typeof(TestParams).GetProperty(nameof(TestParams.Three));
            Set_ParameterValueResolver_Up<TestParams>(property1, null);
            Set_ParameterValueResolver_Up<TestParams>(property2, null);
            Set_ParameterValueResolver_Up<TestParams>(property3, null);

            Assert.Throws<ParameterGroupException>(
                () => target.GetParameters<TestParams>(fakeArgs.Object));
        }

        [TestCase("something", "nothing")]
        [TestCase("", "")]
        public void When__NoOtherParam_Has_TheSameKey__Must_Reject(string value1, string value2)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParamsDifferentKeys.One));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParamsDifferentKeys.Two));
            Set_ParameterValueResolver_Up<TestParams>(property1, value1);
            Set_ParameterValueResolver_Up<TestParams>(property2, value2);

            Assert.Throws<InvalidOperationException>(
                () => target.GetParameters<TestParamsDifferentKeys>(fakeArgs.Object));
        }

        void Set_ParameterValueResolver_Up<TParams>(PropertyInfo property, object value)
        {
            Set_ParameterValueResolver_Up<TParams>(resolver, property, value);
        }

        public static void Set_ParameterValueResolver_Up<TParams>(Mock<IParameterValueResolver> resolver, PropertyInfo property, object value)
        {
            resolver.Setup(
                x => x.ReadParameter<TParams>(
                    It.Is<PropertyInfo>(
                        arg => arg == property),
                    It.IsAny<IDictionary<string, string[]>>(),
                    It.IsAny<TParams>(),
                    It.IsAny<bool>()))
                .Callback<PropertyInfo, IDictionary<string, string[]>, TParams, bool>(
                    (property, b, instance, d) => property.SetValue(instance, value));
        }


        class TestParams
        {
            [Parameter("arg1")]
            [EitherOr("key1")]
            public string One { get; set; }

            [Parameter("arg2")]
            [EitherOr("key1")]
            public int? Two { get; set; }

            [Parameter("arg3")]
            [EitherOr("key1")]
            public string[] Three { get; set; }
        }

        class TestParamsDifferentKeys
        {
            [Parameter("arg1")]
            [EitherOr("key1")]
            public string One { get; set; }

            [Parameter("arg2")]
            [EitherOr("key2")]
            public string[] Two { get; set; }
        }
    }
}