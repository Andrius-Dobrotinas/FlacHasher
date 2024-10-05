using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class EitherOr_TypeCombo_Tests
    {
        ParameterReader target;
        Mock<ParameterValueResolver> resolver;

        public EitherOr_TypeCombo_Tests()
        {
            resolver = new Mock<ParameterValueResolver>();
            target = new ParameterReader(resolver.Object);
        }

        [TestCase("0")]
        [TestCase("a value with spaces")]
        public void One_Parameter_HasValue__Other_IsNotPresent__Must_SetTheValue(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { value } }
            };
            var result = target.GetParameters<TestParams>(argvs);

            Assert.AreEqual(value, result.One);
            Assert.IsNull(result.Two);
        }

        [TestCase("10")]
        [TestCase("another value", "two")]
        public void One_Parameter_HasValue__Other_IsNotPresent__Must_SetTheValue__Case2(params string[] value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg2", new [] { string.Join(";", value) } }
            };
            var result = target.GetParameters<TestParams>(argvs);

            Assert.AreEqual(value, result.Two);
            Assert.IsNull(result.One);
        }

        [TestCase("one value", "another value")]
        [TestCase("a", "value", "too")]
        public void Both_Paramters_HaveValues__Must_Reject(string firstArgValue, params string[] secondArgValue)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { firstArgValue } },
                { "arg2", new [] { string.Join(";", secondArgValue) } }
            };
            Assert.Throws<ParameterGroupException>(
                () => target.GetParameters<TestParams>(argvs));
        }

        [TestCase(null, "goode")]
        [TestCase("value good", null)]
        [TestCase("value good", "")]
        [TestCase("", "alright")]
        public void One_Parameter_HasNoValue__Must_Reject(string first, string second)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new string[] { first } },
                { "arg2", new string[] { second } }
            };
            Assert.Throws<ParameterEmptyException>(
                () => target.GetParameters<TestParams>(argvs));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Neither_Parameter_HasValue__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new string[] { value } },
                { "arg2", new string[] { value } }
            };
            Assert.Throws<ParameterEmptyException>(
                () => target.GetParameters<TestParams>(argvs));
        }
        
        class TestParams
        {
            [Parameter("arg1")]
            [EitherOr("key1")]
            public string One { get; set; }

            [Parameter("arg2")]
            [EitherOr("key1")]
            public string[] Two { get; set; }
        }
    }
}