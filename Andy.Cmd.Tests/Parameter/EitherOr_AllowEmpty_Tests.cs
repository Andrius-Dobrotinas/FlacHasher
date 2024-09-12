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
        public void One_Parameter_HasValue__Other_IsNotPresent__Must_SetTheValue(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { value } }
            };
            var result = ParameterReader.GetParameters<TestParamsEmpties>(argvs);

            Assert.AreEqual(value, result.One);
            Assert.IsNull(result.Two);
        }

        [TestCase("")]
        [TestCase("non-empty")]
        public void One_Parameter_HasValue__Other_IsNotPresent__Must_SetTheValue__Case2(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg2", new [] { value } }
            };
            var result = ParameterReader.GetParameters<TestParamsEmpties>(argvs);

            Assert.AreEqual(value, result.Two);
            Assert.IsNull(result.One);
        }

        [TestCase(null, "goode")]
        [TestCase("value good", null)]
        public void One_Parameter_HasNoValue__Must_Reject(string first, string second)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new string[] { first } },
                { "arg2", new string[] { second } }
            };
            Assert.Throws<ParameterEmptyException>(
                () => ParameterReader.GetParameters<TestParamsEmpties>(argvs));
        }

        [TestCase("", "")]
        [TestCase("", "abc")]
        [TestCase("cda", "")]
        [TestCase("cda", "ax")]
        public void Both_Paramters_HaveValues__Must_Reject(string firstArgValue, string secondArgValue)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { firstArgValue } },
                { "arg2", new [] { secondArgValue } }
            };
            Assert.Throws<ParameterGroupException>(
                () => ParameterReader.GetParameters<TestParamsEmpties>(argvs));
        }

        [Test]
        public void Neither_Parameter_HasValue__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new string[] { null } },
                { "arg2", new string[] { null } }
            };
            Assert.Throws<ParameterEmptyException>(
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