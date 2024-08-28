using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class EitherOr_Tests
    {
        [TestCase("0")]
        [TestCase("a value with spaces")]
        public void One_Parameter_HasValue__Other_DoesNot__Must_SetTheValue(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg1", value }
            };
            var result = ParameterReader.GetParameters<TestParams>(argvs);

            Assert.AreEqual(value, result.One);
            Assert.IsNull(result.Two);
        }

        [TestCase("10")]
        [TestCase("another value")]
        public void One_Paramter_IsEmpty_Other_HasValue__Must_SetTheValue(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg2", value }
            };
            var result = ParameterReader.GetParameters<TestParams>(argvs);

            Assert.AreEqual(value, result.Two);
            Assert.IsNull(result.One);
        }

        [TestCase("one value", "another value")]
        [TestCase("a", "value, too")]
        public void Both_Paramters_HaveValues__Must_Reject(string firstArgValue, string secondArgValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg1", firstArgValue },
                { "arg2", secondArgValue }
            };
            Assert.Throws<ParameterGroupException>(
                () => ParameterReader.GetParameters<TestParams>(argvs));
        }

        [Test]
        public void Neither_Parameter_HasValue__Must_Reject()
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg1", null },
                { "arg2", null }
            };
            Assert.Throws<ParameterGroupException>(
                () => ParameterReader.GetParameters<TestParams>(argvs));
        }

        [TestCase("")]
        public void EmptyString_Not_Accepted(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg1", value }
            };
            Assert.Throws<ParameterEmptyException>(
                () => ParameterReader.GetParameters<TestParams>(argvs));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("a value with spaces")]
        public void When__NoOtherParamHasTheSameKey__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg1", value }
            };
            Assert.Throws<InvalidOperationException>(
                () => ParameterReader.GetParameters<TestParams2>(argvs));
        }

        class TestParams
        {
            [Parameter("arg1")]
            [EitherOr("key1")]
            public string One { get; set; }

            [Parameter("arg2")]
            [EitherOr("key1")]
            public string Two { get; set; }
        }

        class TestParams2
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