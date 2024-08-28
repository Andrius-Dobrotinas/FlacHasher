using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class EitherOr_AllowEmpty_Tests
    {
        [TestCase("")]
        [TestCase("non-empty")]
        public void One_Parameter_HasValue__Other_DoesNot__Must_SetTheValue(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg1", value }
            };
            var result = ParameterReader.GetParameters<TestParamsEmpties>(argvs);

            Assert.AreEqual(value, result.One);
            Assert.IsNull(result.Two);
        }

        [TestCase("")]
        [TestCase("non-empty")]
        public void One_Paramter_IsEmpty_Other_HasValue__Must_SetTheValue(string value)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg2", value }
            };
            var result = ParameterReader.GetParameters<TestParamsEmpties>(argvs);

            Assert.AreEqual(value, result.Two);
            Assert.IsNull(result.One);
        }

        [TestCase("", "")]
        [TestCase("", "abc")]
        [TestCase("cda", "")]
        [TestCase("cda", "ax")]
        public void Both_Paramters_HaveValues__Must_Reject(string firstArgValue, string secondArgValue)
        {
            var argvs = new Dictionary<string, string>
            {
                { "arg1", firstArgValue },
                { "arg2", secondArgValue }
            };
            Assert.Throws<ParameterGroupException>(
                () => ParameterReader.GetParameters<TestParamsEmpties>(argvs));
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
                () => ParameterReader.GetParameters<TestParamsEmpties>(argvs));
        }

        class TestParamsEmpties
        {
            [Parameter("arg1")]
            [EitherOr("key1")]
            [AllowEmpty]
            public string One { get; set; }

            [Parameter("arg2")]
            [EitherOr("key1")]
            [AllowEmpty]
            public string Two { get; set; }
        }
    }
}