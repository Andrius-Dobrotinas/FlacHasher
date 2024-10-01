using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class AtLeastOneOf_ParameterResolution_Tests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("a value with spaces")]
        public void WithOptional_HavingDefaultValue__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.One));
            Assert.Throws<InvalidOperationException>(() => ParameterReader.ReadParameter(prop, argvs, result));
        }
        
        class TestParams
        {
            [Parameter("arg1")]
            [AtLeastOneOf("key1")]
            [Optional(defaultValue: "something")]
            public string One { get; set; }
        }
    }
}