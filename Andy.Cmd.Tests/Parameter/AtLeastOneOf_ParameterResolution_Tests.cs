using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class AtLeastOneOf_ParameterResolution_Tests
    {
        ParameterValueResolver target = new ParameterValueResolver();

        [TestCase(null)]
        [TestCase("")]
        [TestCase("a value with spaces")]
        public void UsedWith_Optional_HavingDefaultValue__Must_Reject(string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg1", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.One));
            Assert.Throws<InvalidOperationException>(() => target.ReadParameter(prop, argvs, result));
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