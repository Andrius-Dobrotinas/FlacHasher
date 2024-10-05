using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class AtLeastOneOf_Multiple_Tests
    {
        ParameterReader target;
        Mock<ParameterValueResolver> resolver;

        public AtLeastOneOf_Multiple_Tests()
        {
            resolver = new Mock<ParameterValueResolver>();
            target = new ParameterReader(resolver.Object);
        }

        [TestCase("0", "1")]
        [TestCase("a value with spaces", "another one")]
        public void One_Parameter_FromEachGroup_HasValue__Other_IsNotPresent__Must_SetTheValue(string value, string value3)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { value } },
                { "arg3", new [] { value3 } }
            };
            var result = target.GetParameters<TestParams>(argvs);

            Assert.AreEqual(value, result.One);
            Assert.IsNull(result.Two);
            Assert.AreEqual(value3, result.Three);
        }
        
        [TestCase("one value", "another value", "third one")]
        [TestCase("a", "value, too", "xyz")]
        public void All_Paramters_HaveValues__Must_SetTheValues(string firstArgValue, string secondArgValue, string thirdArgValue)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { firstArgValue } },
                { "arg2", new[] { secondArgValue } },
                { "arg3", new[] { thirdArgValue } }
            };
            var result = target.GetParameters<TestParams>(argvs);

            Assert.AreEqual(firstArgValue, result.One);
            Assert.AreEqual(secondArgValue, result.Two);
            Assert.AreEqual(thirdArgValue, result.Three);
        }

        [Test]
        public void Neither_Parameter_IsPresent__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg0", new string[] { "value" } }
            };
            Assert.Throws<ParameterGroupException>(
                () => target.GetParameters<TestParams>(argvs));
        }

        class TestParams
        {
            [Parameter("arg1")]
            [AtLeastOneOf("key1")]
            [AtLeastOneOf("key2")]
            public string One { get; set; }

            [Parameter("arg2")]
            [AtLeastOneOf("key1")]
            public string Two { get; set; }

            [Parameter("arg3")]
            [AtLeastOneOf("key2")]
            public string Three { get; set; }
        }
    }
}