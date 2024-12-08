using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter.EitherOr_Tests.ParameterReader_Tests
{
    public class EitherOrTests_Base<TTestParams, TTestParamsDifferentKeys>
        where TTestParams : TestParams, new()
        where TTestParamsDifferentKeys : TestParamsDifferentKeys, new()
    {
        protected ParameterReader target;
        protected Mock<IParameterValueResolver> resolver;
        protected Mock<IDictionary<string, string[]>> fakeArgs;

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
            var property = typeof(TTestParams).GetProperty(propertyName);
            Set_ParameterValueResolver_Up<TTestParams>(property, value);

            Assert.DoesNotThrow(() => target.GetParameters<TTestParams>(fakeArgs.Object));
        }

        [TestCase("ichi", 1, new[] { "a" })]
        [TestCase("ni", null, new[] { "b" })]
        [TestCase("san", 3, null)]
        [TestCase(null, 667, new[] { "c" })]
        public void More_Than_One_Paramter_HasValues__Must_Reject(string firstArgValue, int? secondArgValue, string[] thirdArgValue)
        {
            var property1 = typeof(TTestParams).GetProperty(nameof(TestParams.One));
            var property2 = typeof(TTestParams).GetProperty(nameof(TestParams.Two));
            var property3 = typeof(TTestParams).GetProperty(nameof(TestParams.Three));
            Set_ParameterValueResolver_Up<TTestParams>(property1, firstArgValue);
            Set_ParameterValueResolver_Up<TTestParams>(property2, secondArgValue);
            Set_ParameterValueResolver_Up<TTestParams>(property3, thirdArgValue);

            Assert.Throws<ParameterGroupException>(
                () => target.GetParameters<TTestParams>(fakeArgs.Object));
        }

        [TestCase("something", "nothing")]
        [TestCase("", "")]
        public void When__NoOtherParam_Has_TheSameKey__Must_Reject(string value1, string value2)
        {
            var property1 = typeof(TTestParamsDifferentKeys).GetProperty(nameof(TestParamsDifferentKeys.One));
            var property2 = typeof(TTestParamsDifferentKeys).GetProperty(nameof(TestParamsDifferentKeys.Two));
            Set_ParameterValueResolver_Up<TTestParamsDifferentKeys>(property1, value1);
            Set_ParameterValueResolver_Up<TTestParamsDifferentKeys>(property2, new[] { value2 });

            Assert.Throws<InvalidOperationException>(
                () => target.GetParameters<TTestParamsDifferentKeys>(fakeArgs.Object));
        }


        protected void Set_ParameterValueResolver_Up<TParams>(PropertyInfo property, object value)
        {
            Util.Set_ParameterValueResolver_Up<TParams>(resolver, property, value);
        }
    }

    public abstract class TestParams
    {
        [Parameter("arg1")]
        [EitherOr("key1")]
        public abstract string One { get; set; }

        [Parameter("arg2")]
        [EitherOr("key1")]
        public abstract int? Two { get; set; }

        [Parameter("arg3")]
        [EitherOr("key1")]
        public abstract string[] Three { get; set; }
    }

    public abstract class TestParamsDifferentKeys
    {
        [Parameter("arg1")]
        [EitherOr("key1")]
        public abstract string One { get; set; }

        [Parameter("arg2")]
        [EitherOr("key2")]
        public abstract string[] Two { get; set; }
    }
}